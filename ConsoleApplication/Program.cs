using NinjaDomain.Classes;
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
            QueryAndUpdateNinja();
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
            using (var context = new NinjaContext())
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
            var keyval = 4;
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                var ninja = context.Ninjas.Find(keyval);
                Console.WriteLine($"After Find1: {ninja.Name}");

                // if the object is in memory, it will not query
                // the db a second time. great for performance
                // EF does this automagically.
                var someNinja = context.Ninjas.Find(keyval);
                Console.WriteLine($"After Find2: {someNinja.Name}");

                ninja = null;
            }
        }

        private static void RetrieveDataWithStoredProc()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                var ninjas = context.Ninjas.SqlQuery("exec GetOldNinjas");
               
                foreach(var ninja in ninjas)
                {
                    Console.WriteLine(ninja.Name);
                }
            }
        }
    }
}
