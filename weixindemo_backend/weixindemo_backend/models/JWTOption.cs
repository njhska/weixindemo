namespace weixindemo_backend.models
{
    public class JWTOption
    {
        public string SigningKey { get; set; }
        public int ExpireSeconds { get; set; }
    }
}
