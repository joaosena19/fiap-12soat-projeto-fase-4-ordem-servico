namespace Infrastructure.Database
{
    /// <summary>
    /// Configurações do MongoDB
    /// </summary>
    public class MongoDbSettings
    {
        /// <summary>
        /// String de conexão do MongoDB
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Nome do banco de dados
        /// </summary>
        public string DatabaseName { get; set; } = string.Empty;
    }
}
