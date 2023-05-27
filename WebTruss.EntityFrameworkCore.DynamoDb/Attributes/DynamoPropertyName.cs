namespace WebTruss.EntityFrameworkCore.DynamoDb.Attributes
{
    public class DynamoPropertyName : Attribute
    {
        public DynamoPropertyName(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
