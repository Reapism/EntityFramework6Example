using NinjaDomain.Classes;
using NinjaDomain.Classes.Enums;
using NinjaDomain.DataModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace ConsoleApplication
{
    public class Program
    {
        private static void Main(string[] args)
        {
            // Stop EF the database initialization process when working with NinjaContext
            Database.SetInitializer(new NullDatabaseInitializer<NinjaContext>());

            SimpleNinjaGraphQuery();

            Console.ReadKey();
        }

        private static void InsertNinja()
        {
            Ninja ninja = new Ninja
            {
                Name = "SampsonSan",
                ServedInOniwaban = false,
                DateOfBirth = new DateTime(2008, 1, 28),
                ClanId = 1
            };

            // uses entity framework to add the local ninja obj
            using (NinjaContext context = new NinjaContext())
            {
                // logging
                context.Database.Log = Console.WriteLine;
                context.Ninjas.Add(ninja);
                context.SaveChanges();
            }

        }

        private static void InsertNinjas()
        {
            Ninja ninja1 = new Ninja
            {
                Name = "Anthony",
                ServedInOniwaban = false,
                DateOfBirth = new DateTime(2008, 1, 28),
                ClanId = 1
            };

            Ninja ninja2 = new Ninja
            {
                Name = "Michael",
                ServedInOniwaban = false,
                DateOfBirth = new DateTime(2008, 1, 28),
                ClanId = 1
            };

            // uses entity framework to add the local ninja obj
            using (NinjaContext context = new NinjaContext())
            {
                // logging
                context.Database.Log = Console.WriteLine;
                context.Ninjas.AddRange(new List<Ninja> { ninja1, ninja2 });
                context.SaveChanges();
            }

        }

        private static void SimpleNinjaQueries()
        {
            using (NinjaContext context = new NinjaContext())
            {
                List<Ninja> ninjas = context.Ninjas.ToList();




                //DbSet<Ninja> query = context.Ninjas;
                // var someninjas = query.ToList();


                // avoid doing large queries like this
                // note: the connection is open this entire
                // time
                //foreach (Ninja ninja in context.Ninjas)
                //{
                //    Console.WriteLine(ninja.Name);
                //}
            }
        }

        private static void SimpleNinjaQueries2()
        {
            using (NinjaContext context = new NinjaContext())
            {
                // get ninjas where the name is anthony
                IQueryable<Ninja> ninjas = context.Ninjas.Where(n => n.Name == "Anthony");

                //gets only one ninja named anthony. hardcoded
                Ninja oneninja = context.Ninjas.Where(n => n.Name == "Anthony").FirstOrDefault();

                // if anthony is replaced with a variable, the query behind the scenes is 
                // always parameterized (prevents SQL injection)


                // First is going to execute immediatelty, and is not great at performance
                // Put your executing method at the end, if you put it in the front before
                // writing orderby and skip, those actions will then be performed in memory (bad performance)
                Ninja optimized_ninja = context.Ninjas.
                    Where(n => n.DateOfBirth >= new DateTime(1984, 1, 1))
                    .OrderBy(n => n.Name) // sorting
                    .Skip(1).Take(1) // paging
                    .FirstOrDefault(); // the executing part of the query needed to actually run the query

                foreach (Ninja ninja in ninjas)
                {
                    Console.WriteLine(ninja.Name);
                }
            }
        }

        private static void QueryAndUpdateNinja()
        {
            using (NinjaContext context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                Ninja ninja = context.Ninjas.FirstOrDefault();

                // update the instance will create a corresponding
                // update for sql.
                ninja.ServedInOniwaban = (!ninja.ServedInOniwaban);
                context.SaveChanges();
            }
        }

        /// <summary>
        ///  e.g. you have a website, service, or web api
        ///  where you called into that service to retrieve
        ///  that data, you do some work on it for a client,
        ///  and then you send it back
        /// </summary>
        private static void QueryAndUpdateNinjaDisconnected()
        {
            Ninja ninja;
            using (NinjaContext context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                ninja = context.Ninjas.FirstOrDefault();
            }

            // notice this change is made outside of the connection
            // connection is opened and closed on the using
            // scope.
            ninja.ServedInOniwaban = (!ninja.ServedInOniwaban);

            // this is a brand new instance of the context
            using (NinjaContext context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                // this still doesn't notify this new context instance
                // of the changes made to ninja outside the first context
                // instance
                context.Ninjas.Attach(ninja);

                // Giving a parameter to the Entry function tells it which object
                // you want to use, and then the State property is used
                // to notify this new context with the state of the object
                context.Entry(ninja).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        private static void RetrieveDataWithFind()
        {
            int keyval = 4;
            using (NinjaContext context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                Ninja ninja = context.Ninjas.Find(keyval);
                Console.WriteLine($"After Find1: {ninja.Name}");

                // if the object is in memory, it will not query
                // the db a second time. great for performance
                // EF does this automagically.
                Ninja someNinja = context.Ninjas.Find(keyval);
                Console.WriteLine($"After Find2: {someNinja.Name}");

                ninja = null;
            }
        }

        private static void RetrieveDataWithStoredProc()
        {
            using (NinjaContext context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                System.Data.Entity.Infrastructure.DbSqlQuery<Ninja> ninjas = context.Ninjas.SqlQuery("exec GetOldNinjas");

                foreach (Ninja ninja in ninjas)
                {
                    Console.WriteLine(ninja.Name);
                }

                // or use the LINQ execution methods

                List<Ninja> ninjas2 = context.Ninjas.SqlQuery("exec GetOldNinjas").ToList();
            }
        }

        private static void DeleteNinja()
        {
            using (NinjaContext context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                Ninja ninja = context.Ninjas.FirstOrDefault();
                context.Ninjas.Remove(ninja);
                context.SaveChanges();
            }
        }

        private static void DeleteNinjaCommon()
        {
            Ninja ninja;
            using (NinjaContext context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                ninja = context.Ninjas.FirstOrDefault();
            }

            // 1.] this is a new context, so it can't find the ninja
            // object that was initially queried for.
            using (NinjaContext context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;

                // 2.] however, if you attach it
                // to let this db context instance 
                // it will now understand.
                context.Ninjas.Attach(ninja);
                ninja = context.Ninjas.Remove(ninja);


                // even better is to use context.entry
                // it combines the two statements above
                context.Entry(ninja).State = EntityState.Deleted;
                context.SaveChanges();
            }
        }

        private static void DeleteNinjaViaStoredProc()
        {
            var keyval = 3;
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                context.Database.ExecuteSqlCommand("exec DeleteNinjaViaId {0}", keyval);
            }
        }

        private static void InsertNinjaWithEquipment()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;

                var ninja = new Ninja
                {
                    Name = "John Smith",
                    ServedInOniwaban = false,
                    DateOfBirth =  new DateTime(1990, 1,14),
                    ClanId = 1
                };

                var muscles = new NinjaEquipment
                {
                    Name = "Muscles",
                    Type = EquipmentType.Tool
                };

                var scythe = new NinjaEquipment
                {
                    Name = "Scythe",
                    Type = EquipmentType.Weapon
                };

                // All 3 of these statements were
                // executed in the same DB connection

                context.Ninjas.Add(ninja);
                ninja.EquipmentOwned.Add(muscles);
                ninja.EquipmentOwned.Add(scythe);
                context.SaveChanges();
            }
        }

        private static void SimpleNinjaGraphQuery()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;

                // Eagar Loading way: (using include)
                // brings all the data back in one call!
                // 
                // include will say get this additional
                // however, using multiple ones, will degrades
                // as you add more.
                // request the equipment from this ninja
                var ninja = context.Ninjas.Include(n => n.EquipmentOwned)
                    .FirstOrDefault(n => n.Name.StartsWith("John"));

                Console.WriteLine($"Ninja Retrieved: {ninja.Name}");

                // Explicit Loading way: (using.Entry().Collection)
                // Load executes it right away similar to a query execution statement
                // like FirstOrDefault()
                context.Entry(ninja).Collection(n => n.EquipmentOwned).Load();

                // Lazy Loading:
                // See screenshot "lazy loading"
                // Essentially mark the property you want to retrieve
                // data from as virtual.
                // there could be lots of performance issue.
                // commenting out the context.entry above.
                Console.WriteLine($"Ninja Equipment Count: {ninja.EquipmentOwned.Count}");
            }
        }


    }
}
