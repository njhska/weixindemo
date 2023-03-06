using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using weixindemo_backend.models;

namespace weixindemo_backend.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class HomeController : ControllerBase
    {
        private static readonly string appid = "wxfc99888356b98d01";
        private static readonly string secret = "a6179c22299ff74f39a4e99feeb6c857";

        private static Dictionary<string, UserInfo> users = new Dictionary<string, UserInfo>();

        private readonly ILogger<HomeController> logger;
        private readonly IOptions<JWTOption> jwtOption;

        public HomeController(ILogger<HomeController> logger,IOptions<JWTOption> jwtOption)
        {
            this.logger = logger;
            this.jwtOption = jwtOption;
        }

        [HttpPost]
        public async Task<IActionResult> SignUp([FromQuery]string code,UserInfo userInfo)
        {
            if(string.IsNullOrEmpty(code))
            {
                logger.LogError($"×¢²áÊ§°Ü,{nameof(code)}Îª¿Õ");
                return BadRequest();
            }

            var openId = await GetWeixinOpenId(code);
            if (string.IsNullOrEmpty(openId))
            {
                logger.LogError($"×¢²áÊ§°Ü,{nameof(openId)}Îª¿Õ");
                return BadRequest();
            }
            users.Add(openId, userInfo);
            logger.LogInformation(code + "×¢²á³É¹¦");
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Login(string code)
        {
            if(string.IsNullOrEmpty(code))
            {
                logger.LogError($"µÇÂ½Ê§°Ü,{nameof(code)}Îª¿Õ");
                return BadRequest();
            }
            var openId = await GetWeixinOpenId(code);
            if (string.IsNullOrEmpty(openId))
            {
                logger.LogError($"µÇÂ½Ê§°Ü,{nameof(openId)}Îª¿Õ");
                return BadRequest();
            }
            if (!users.ContainsKey(openId))
            {
                logger.LogError($"µÇÂ½Ê§°Ü,Ã»ÓÐÕÒµ½{openId}¶ÔÓ¦µÄÕË»§");
                return NotFound(openId);
            }
            var claims = new List<Claim>();
            var user = users[openId];
            claims.Add(new Claim("name", user.nickName));
            claims.Add(new Claim("gender", user.Sex));
            claims.Add(new Claim("avatar", user.avatarUrl));

            string jwtToken = BuildToken(claims, jwtOption.Value);
            logger.LogInformation(openId+"µÇÂ½³É¹¦");
            return Ok(jwtToken);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Index()
        {
            var claims = User.Claims.ToList();
            return Ok(
                new {
                    name = claims[0].Value,
                    gender = claims[1].Value,
                    avatar = claims[2].Value
                }
                );
        }

        private static async Task<string> GetWeixinOpenId(string code)
        {
            HttpClient client = new HttpClient();
            var response = await client
                .GetFromJsonAsync<UserToken>($"https://api.weixin.qq.com/sns/jscode2session?appid={appid}&secret={secret}&js_code={code}&grant_type=authorization_code");
            if(response!=null)
            {
                return response.openid;
            }
            return string.Empty;

        }

        private static string BuildToken(IEnumerable<Claim> claims, JWTOption options)
        {
            DateTime expires = DateTime.Now.AddSeconds(options.ExpireSeconds);
            byte[] keyBytes = Encoding.UTF8.GetBytes(options.SigningKey);
            var secKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(secKey,
                SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new JwtSecurityToken(expires: expires,
                signingCredentials: credentials, claims: claims);
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}