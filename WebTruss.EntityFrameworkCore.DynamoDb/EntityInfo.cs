using Amazon.Runtime.Credentials.Internal;
using System.Reflection;
using WebTruss.EntityFrameworkCore.DynamoDb.Attributes;

namespace WebTruss.EntityFrameworkCore.DynamoDb
{
    public class EntityInfo
    {
        public EntityInfo(Type type, string tableName, DynamoDbContext context)
        {
            TableName = tableName;
            Properties = new List<DynamoPropertyInfo>();
            LinkedEntities = new List<DynamoEntityLink>();
            Dictionary<PropertyInfo, DynamoLink> toBeLinked = new();
            foreach (var property in type.GetProperties())
            {
                var dynoIgnoreAttribute = property.GetCustomAttribute<DynamoIgnore>();
                if (dynoIgnoreAttribute != null)
                {
                    continue;
                }

                var linkAttribute = property.GetCustomAttribute<DynamoLink>();
                if (linkAttribute != null)
                {
                    toBeLinked.Add(property, linkAttribute);
                    continue;
                }

                DynamoPropertyType? dynamoPropertyType = null;
                if (property.GetCustomAttribute<Pk>() != null)
                {
                    dynamoPropertyType = DynamoPropertyType.Pk;
                }
                else if (property.GetCustomAttribute<Sk>() != null)
                {
                    dynamoPropertyType = DynamoPropertyType.Sk;
                }
                else
                {
                    dynamoPropertyType = DynamoPropertyType.Other;
                }
                var dynamoPropertyNameAttribute = property.GetCustomAttribute<DynamoPropertyName>();
                if (dynamoPropertyNameAttribute == null)
                {
                    Properties.Add(new DynamoPropertyInfo(property, property.Name, dynamoPropertyType.Value));
                }
                else
                {
                    Properties.Add(new DynamoPropertyInfo(property, dynamoPropertyNameAttribute.Name, dynamoPropertyType.Value));
                }
            }

            if (!Properties.Where(a => a.DynamoPropertyType == DynamoPropertyType.Pk).Any())
            {
                throw new Exception("Provided entity does not have a property marked with pk attribute.");
            }

            if (toBeLinked.Any())
            {
                Link(toBeLinked, context);
            }
        }

        public string TableName { get; set; } = null!;
        public List<DynamoPropertyInfo> Properties { get; set; } = null!;
        public List<DynamoEntityLink> LinkedEntities { get; set; } = null!;

        public DynamoPropertyInfo Pk()
        {
            return Properties.Where(x => x.DynamoPropertyType == DynamoPropertyType.Pk).First();
        }

        public DynamoPropertyInfo? Sk()
        {
            return Properties.Where(x => x.DynamoPropertyType == DynamoPropertyType.Sk).FirstOrDefault();
        }

        private void Link(Dictionary<PropertyInfo, DynamoLink> toBeLinked, DynamoDbContext context)
        {
            foreach (var item in toBeLinked)
            {
                var linkingPropertyInfo = Properties.Where(x=> x.Name == item.Value.LinkingAttributeName).FirstOrDefault();
                if(linkingPropertyInfo == null)
                {
                    throw new Exception($"Linking attribute {item.Value.LinkingAttributeName} not found.");
                }

                var link = LinkedEntities.Where(x => x.LinkingName == item.Value.LinkingAttributeName).FirstOrDefault();
                if (link == null)
                {
                    var entityPkProperty = item.Value.EntityType.GetProperties().Where(x=> x.GetCustomAttribute<Pk>() != null).First();
                    var entityPkPropertyName = entityPkProperty.GetCustomAttribute<DynamoPropertyName>();

                    var entitySkProperty = item.Value.EntityType.GetProperties().Where(x => x.GetCustomAttribute<Sk>() != null).FirstOrDefault();
                    var entitySkPropertyName = entitySkProperty?.GetCustomAttribute<DynamoPropertyName>() ?? null;
                    LinkedEntities.Add(new DynamoEntityLink
                    {
                        TableName = context.GetTableName(item.Value.EntityType),
                        Properties = new List<DynamoPropertyInfo> { new DynamoPropertyInfo(item.Key, item.Value.AttributeName ?? item.Key.Name, DynamoPropertyType.Other) },
                        PkValue = item.Value.PkValue,
                        LinkingName = item.Value.LinkingAttributeName,
                        PkPropertyInfo = new DynamoPropertyInfo(entityPkProperty, entityPkPropertyName?.Name ?? entityPkProperty.Name, DynamoPropertyType.Pk)
                    });
                }
                else
                {
                    link.Properties.Add(new DynamoPropertyInfo(item.Key, item.Value.AttributeName ?? item.Key.Name, DynamoPropertyType.Other));
                }

            }
        }
    }

    public class DynamoPropertyInfo
    {
        public DynamoPropertyInfo(PropertyInfo property, string name, DynamoPropertyType dynamoPropertyType)
        {
            Property = property;
            Name = name;
            DynamoPropertyType = dynamoPropertyType;
        }

        public PropertyInfo Property { get; set; } = null!;
        public string Name { get; set; } = null!;
        public DynamoPropertyType DynamoPropertyType { get; set; }
    }

    public class DynamoEntityLink
    {
        public string TableName { get; set; } = null!;
        public string LinkingName { get; set; } = null!;
        public object? PkValue { get; set; }
        public DynamoPropertyInfo PkPropertyInfo { get; set; } = null!;
        public DynamoPropertyInfo? SkPropertyInfo { get; set; } = null!;
        public List<DynamoPropertyInfo> Properties { get; set; } = null!;
    }
}
