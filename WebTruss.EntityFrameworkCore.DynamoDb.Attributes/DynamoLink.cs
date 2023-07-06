namespace WebTruss.EntityFrameworkCore.DynamoDb.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DynamoLink : Attribute
    {
        public string LinkingAttributeName { get; set; } = null!;
        public Type EntityType { get; set; }
        public string? AttributeName { get; set; }
        public object? PkValue { get; set; }


        public DynamoLink(object pkValue, string linkingAttribute, Type entityType, string attributeName)
        {
            PkValue = pkValue;
            LinkingAttributeName = linkingAttribute;
            EntityType = entityType;
            AttributeName = attributeName;
        }

        public DynamoLink(object pkValue, string linkingAttribute, Type entityType)
        {
            PkValue = pkValue;
            LinkingAttributeName = linkingAttribute;
            EntityType = entityType;
        }

        public DynamoLink(string linkingAttribute, Type entityType)
        {
            LinkingAttributeName = linkingAttribute;
            EntityType = entityType;
        }

        public DynamoLink(string linkingAttribute, Type entityType, string attributeName)
        {
            LinkingAttributeName = linkingAttribute;
            EntityType = entityType;
            AttributeName = attributeName;
        }
    }
}
