namespace weixindemo_backend.models
{
    public class UserInfo
    {
        public string avatarUrl { get; set; }
        public string city { get; set; }
        public int gender { get; set; }
        public string Sex
        {
            get { return gender == 0 ? "男" : "女"; }
        }
        public string nickName { get; set; }
    }
}
