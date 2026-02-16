using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;

namespace Infrastructure.Database
{
    /// <summary>
    /// Configuração de mapeamento do MongoDB
    /// Como usamos Documents (DTOs) no repositório, apenas convenções globais são necessárias
    /// </summary>
    public static class MongoDbMappingConfiguration
    {
        private static bool _isConfigured = false;
        private static readonly object _lock = new();

        /// <summary>
        /// Registra as convenções globais do MongoDB
        /// </summary>
        public static void Configure()
        {
            lock (_lock)
            {
                if (_isConfigured)
                    return;

                // Configurar convenções globais
                var conventionPack = new ConventionPack
                {
                    new IgnoreExtraElementsConvention(true), // Ignora campos extras no documento
                    new EnumRepresentationConvention(BsonType.String) // Serializa enums como string
                };
                ConventionRegistry.Register("DefaultConventions", conventionPack, _ => true);

                _isConfigured = true;
            }
        }
    }
}
