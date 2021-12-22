namespace Customers.API
{
    public static class DbInitializer
    {
        public static void Initialize(CustomerDBContext context)
        {
            if (context.Customers.Any())
            {
                return;   // DB has been seeded
            }

            var customers = new Customer[]
            {
                new Customer{Name="John", CustomerUniqueId = "CX111000"},
                new Customer{Name="Carson", CustomerUniqueId = "CX111040"},
            };

            context.Customers.AddRange(customers);
            context.SaveChanges();
        }
    }
}
