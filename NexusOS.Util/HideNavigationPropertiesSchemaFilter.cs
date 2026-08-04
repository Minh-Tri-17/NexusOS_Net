using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace NexusOS.Util
{
    public class HideNavigationPropertiesSchemaFilter : ISchemaFilter
    {
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema?.Properties == null)
                return;

            var navigationProperties = context.Type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p =>
                    // Có virtual (EF navigation thường là virtual)
                    p.GetMethod?.IsVirtual == true
                    ||
                    // Có ForeignKey attribute
                    p.GetCustomAttribute<ForeignKeyAttribute>() != null
                    ||
                    // Có InverseProperty attribute
                    p.GetCustomAttribute<InversePropertyAttribute>() != null)
                .Select(p => char.ToLowerInvariant(p.Name[0]) + p.Name.Substring(1));

            foreach (var prop in navigationProperties)
            {
                if (schema.Properties.ContainsKey(prop))
                    schema.Properties.Remove(prop);
            }
        }
    }
}
