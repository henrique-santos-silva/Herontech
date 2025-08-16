using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Herontech.Infrastructure.Persistence;

using System.Text.RegularExpressions;

public static class ModelBuilderExtensions
{
    public static void UseSnakeCaseNames(this ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            // Nome da tabela
            entity.SetTableName(ToSnakeCase(entity.GetTableName()!));

            // Nomes das colunas
            foreach (var property in entity.GetProperties())
                property.SetColumnName(ToSnakeCase(property.GetColumnName(StoreObjectIdentifier.Table(entity.GetTableName()!, null))!));

            // Nomes das chaves
            foreach (var key in entity.GetKeys())
                key.SetName(ToSnakeCase(key.GetName()!));

            // Nomes das FKs
            foreach (var fk in entity.GetForeignKeys())
                fk.SetConstraintName(ToSnakeCase(fk.GetConstraintName()!));

            // Nomes dos índices
            foreach (var index in entity.GetIndexes())
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()!));
        }
    }

    private static string ToSnakeCase(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return name;

        var startUnderscores = Regex.Match(name, @"^_+");
        return startUnderscores + Regex.Replace(name, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
    }
}
