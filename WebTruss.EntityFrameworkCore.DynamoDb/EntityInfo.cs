using System.Reflection;
using WebTruss.EntityFrameworkCore.DynamoDb.Attributes;

namespace WebTruss.EntityFrameworkCore.DynamoDb
{
    public class EntityInfo
    {
        public EntityInfo(Type type, string tableName)
        {
            TableName = tableName;
            Properties = new List<DynamoPropertyInfo>();
            foreach (var property in type.GetProperties())
            {
                var dynoIgnoreAttribute = property.GetCustomAttribute<DynamoIgnore>();
                if (dynoIgnoreAttribute != null)
                {
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
        }

        public string TableName { get; set; } = null!;
        public List<DynamoPropertyInfo> Properties { get; set; } = null!;

        public DynamoPropertyInfo Pk()
        {
            return Properties.Where(x => x.DynamoPropertyType == DynamoPropertyType.Pk).First();
        }

        public DynamoPropertyInfo? Sk()
        {
            return Properties.Where(x => x.DynamoPropertyType == DynamoPropertyType.Sk).FirstOrDefault();
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


}
