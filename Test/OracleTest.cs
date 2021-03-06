using System;
using System.Collections;
using System.IO;
using System.Linq;
using ALinq.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;

namespace Test
{
    [TestClass]
    public class NorOracleTest : SqlTest
    {
        protected static TextWriter writer;

        public override NorthwindDatabase CreateDataBaseInstace()
        {
            //Console.OutputEncoding = Encoding.GetEncoding("gb2312");
            //writer = new StreamWriter("c:/1.txt",false,Encoding.GetEncoding("gb2312"));
            //var xmlMapping = XmlMappingSource.FromStream(GetType().Assembly.GetManifestResourceStream("Test.Northwind.Oracle.map"));
            writer = Console.Out;
            return new OracleNorthwind(CreateConnection()) { Log = writer };
        }

        protected System.Data.Common.DbConnection CreateConnection()
        {
            return OracleNorthwind.CreateConnection("Northwind", "Test", "vpc1");
        }

        [ClassInitialize]
        public static void Initialize(TestContext testContext)
        {
            //var type = typeof(SQLiteTest);
            //var path = type.Module.FullyQualifiedName;
            //var filePath = Path.GetDirectoryName(path) + @"\ALinq.Oracle.lic";
            //File.Copy(@"E:\ALinqs\ALinq1.8\Test\ALinq.Oracle.lic", filePath);
            //filePath = Path.GetDirectoryName(path) + @"\Northwind.Oracle.map";
            //File.Copy(@"E:\ALinqs\ALinq1.8\Test\Northwind.Oracle.map", filePath);

            //writer = new StreamWriter("c:/Oracle.txt", false);
            //var database = new OracleNorthwind(OracleNorthwind.CreateConnection("Northwind", "Northwind", "localhost")) { Log = writer };
            //if (!database.DatabaseExists())
            //{
            //    database.CreateDatabase();
            //    database.Connection.Close();
            //}
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            writer.Flush();
            writer.Close();
        }

        [TestMethod]
        public override void CreateDatabase()
        {
            //writer = new StreamWriter("c:/Oracle.txt", false);
            //var xmlMapping = XmlMappingSource.FromUrl("Northwind.Oracle.map");
            //var database = new OracleNorthwind(OracleNorthwind.CreateConnection("System", "test", "localhost"), xmlMapping) { Log = Console.Out };
            var database = new OracleNorthwind(OracleNorthwind.CreateConnection("System", "test", "vpc1")) { Log = Console.Out };
            if (database.DatabaseExists())
                database.DeleteDatabase();
            try
            {
                database.CreateDatabase();
            }
            catch (Exception)
            {
                database.Log.Flush();
                database.Log.Close();
                throw;
            }
        }

        [TestMethod]
        public new void OrderBy_Simple()
        {
            IList items = (from e in db.Employees
                           orderby e.HireDate
                           select e).ToList();

            db.Employees.OrderBy(o => o.HireDate).ThenBy(o => o.LastName).ToArray();

            Assert.IsTrue(items.Count > 0);
            for (int i = 1; i < items.Count; i++)
            {
                var preItem = (Employee)items[i - 1];
                var current = (Employee)items[i];
                Assert.IsTrue((preItem.HireDate ?? DateTime.MaxValue) <= (current.HireDate ?? DateTime.MaxValue));
            }
        }

        public static Func<OracleNorthwind, string, IQueryable<Customer>> CustomersByCity =
            ALinq.CompiledQuery.Compile((OracleNorthwind db, string city) =>
                                        from c in db.Customers
                                        where c.City == city
                                        select c);
        public static Func<OracleNorthwind, string, Customer> CustomersById =
            ALinq.CompiledQuery.Compile((OracleNorthwind db, string id) =>
                                               Enumerable.Where(db.Customers, c => c.CustomerID == id).First());

        [TestMethod]
        public virtual void StoreAndReuseQuery()
        {
            var customers = CustomersByCity((OracleNorthwind)db, "London").ToList();
            Assert.IsTrue(customers.Count() > 0);
            var id = customers.First().CustomerID;
            var customer = CustomersById((OracleNorthwind)db, id);
            Assert.AreEqual("London", customer.City);
        }

        [TestMethod]
        public virtual void StringConnect()
        {
            var connstr = CreateConnection().ConnectionString;
            var context = new ALinq.DataContext(connstr, typeof(ALinq.Oracle.OracleProvider));
            context.Connection.Open();
            context.Connection.Close();
        }

        //[TestMethod]
        //public void Procedures_GetCustomersCount()
        //{
        //    int count = 0;
        //    db.Log = Console.Out;
        //    count = ((OracleNorthwind)db).GetCustomersCount();
        //    Console.WriteLine(count);
        //}

        //[TestMethod]
        //public new void Procedure_AddCategory()
        //{
        //    db.Log = Console.Out;
        //    var categoryID = db.Categories.Max(o => o.CategoryID) + 1;
        //    ((OracleNorthwind)db).AddCategory(categoryID, "category", "description");
        //}

        //存储过程
        //1、标量返回
        //[TestMethod]
        //public new void Procedure_GetCustomersCountByRegion()
        //{
        //    db.Log = Console.Out;
        //    var groups = db.Customers.GroupBy(o => o.Region).Select(g => new { Count = g.Count(), Region = g.Key }).ToArray();
        //    foreach (var group in groups)
        //    {
        //        if (group.Region == null)
        //            continue;
        //        var count1 = group.Count;
        //        var count2 = ((OracleNorthwind)db).GetCustomersCountByRegion(group.Region);
        //        Assert.AreEqual(count1, count2);
        //    }
        //}

        //2、单一结果集返回
        //[TestMethod]
        //public new void Procedure_GetCustomersByCity()
        //{
        //    var groups = db.Customers.GroupBy(o => o.City).Select(g => new { Key = g.Key, Count = g.Count() }).ToArray();
        //    foreach (var group in groups)
        //    {
        //        if (group.Key == null)
        //            continue;
        //        var result = ((OracleNorthwind)db).GetCustomersByCity(group.Key, null).ToArray();
        //        Assert.AreEqual(group.Count, result.Count());
        //    }
        //}

        //3.多个可能形状的单一结果集
        //[TestMethod]
        //public void Procedure_SingleRowset_MultiShape()
        //{
        //    //返回全部Customer结果集
        //    var result = ((OracleNorthwind)db).SingleRowset_MultiShape(1, null);
        //    var shape1 = result.GetResult<Customer>();
        //    foreach (var compName in shape1)
        //    {
        //        Console.WriteLine(compName.CompanyName);
        //    }

        //    //返回部分Customer结果集
        //    result = ((OracleNorthwind)db).SingleRowset_MultiShape(2, null);
        //    var shape2 = result.GetResult<NorthwindDemo.OracleNorthwind.PartialCustomersSetResult>();
        //    foreach (var con in shape2)
        //    {
        //        Console.WriteLine(con.ContactName);
        //    }
        //}

        //4.多个结果集
        //[TestMethod]
        //public void Procedure_GetCustomerAndOrders()
        //{
        //    db.Log = Console.Out;
        //    var result = ((OracleNorthwind)db).GetCustomerAndOrders("SEVES", null, null);
        //    //返回Customer结果集
        //    var customers = result.GetResult<Customer>();

        //    //返回Orders结果集
        //    var orders = result.GetResult<Order>();

        //    //在这里，我们读取CustomerResultSet中的数据
        //    foreach (var cust in customers)
        //    {
        //        Console.WriteLine(cust.CustomerID);
        //    }

        //    foreach (var order in orders)
        //    {
        //        Console.WriteLine(order.OrderID);
        //    }
        //}

        [TestMethod]
        public void DataType_Clob()
        {
            var category = db.Categories.FirstOrDefault();
        }

        [TestMethod]
        public void Bitwise()
        {
            var q = db.DataTypes.Select(o => o.ID & 1);
            var command = db.GetCommand(q);
            Console.WriteLine(command.CommandText);

            q = db.DataTypes.Select(o => o.ID | 1);//OR
            command = db.GetCommand(q);
            Console.WriteLine(command.CommandText);

            q = db.DataTypes.Select(o => ~o.ID);//NOT
            command = db.GetCommand(q);
            Console.WriteLine(command.CommandText);

            q = db.DataTypes.Select(o => o.ID ^ 1);//XOR 
            command = db.GetCommand(q);
            Console.WriteLine(command.CommandText);
        }

        [TestMethod]
        public void TempTest()
        {
            //var a = 888888888.00f;
            //Console.WriteLine(a);
            var od = db.OrderDetails.First();
            od.UnitPrice = 888888888.00M;
            db.SubmitChanges();
            Console.WriteLine(od.UnitPrice);
        }
    }
}
