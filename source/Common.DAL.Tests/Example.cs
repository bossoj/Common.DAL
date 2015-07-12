using System;
using System.Configuration;
using System.Linq;
using Common.DAL.EF;
using Common.DAL.Interface;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Common.DAL.Tests
{
    [TestClass]
    public class Example
    {
        private IDbContextFactory<DbContext> dbContextFactory;
        private IDbContextProvider dbContextProvider;
        private IUnitOfWorkFactory unitOfWorkFactory;
        private IRepository<Blog> repositoryBlog;
        private IRepository<Post> repositoryPost;

        //======================================================================================

        [TestInitialize]
        public void TestInitialize()
        {
            var connectionString = ConfigurationManager.AppSettings["SQLSERVER_CONNECTION_STRING"];

            dbContextFactory = new DbContextFactory(connectionString, s => new BlogContext(s));
            dbContextProvider = new ThreadDbContextProvider();
            unitOfWorkFactory = new UnitOfWorkFactory(dbContextFactory, dbContextProvider);
            repositoryBlog = new Repository<Blog>(dbContextProvider);
            repositoryPost = new Repository<Post>(dbContextProvider);

            TestCleanup();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            //using (var uow = unitOfWorkFactory.Create())
            //{
            //    repositoryBlog.DeleteRange(repositoryBlog.GetAll());
            //    repositoryPost.DeleteRange(repositoryPost.GetAll());

            //    uow.Commit();
            //}
        }

        //======================================================================================

        [ClassInitialize]
        static public void ClassInitialize(TestContext context)
        {
            //HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.Initialize();
        }

        [ClassCleanup]
        static public void ClassCleanup()
        {
        }

        //======================================================================================

        List<Blog> blogList = new List<Blog>
        {
            new Blog
            {
                Name = "Blog_1",
                Rating = 5,
                Url = "blog_1.ru"
            },
            new Blog
            {
                Name = "Blog_2",
                Rating = 1,
                Url = "blog_2.ru"
            },
            new Blog
            {
                Name = "Blog_3",
                Rating = 3,
                Url = "blog_3.ru"
            },
            new Blog
            {
                Name = "Blog_4",
                Rating = 8,
                Url = "blog_4.ru"
            } 
        };

        //======================================================================================

        [TestMethod]
        public void Test_Config()
        {
            Console.WriteLine("AppSettings:");
            try
            {
                var appSettings = ConfigurationManager.AppSettings;

                if (appSettings.Count == 0)
                {
                    Console.WriteLine("AppSettings is empty.");
                }
                else
                {
                    foreach (var key in appSettings.AllKeys)
                    {
                        Console.WriteLine("Key: {0} Value: {1}", key, appSettings[key]);
                    }
                }
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings");
            }
            Console.WriteLine();

            Console.WriteLine("ConnectionStrings:");
            try
            {
                var connectionStrings = ConfigurationManager.ConnectionStrings;

                if (connectionStrings.Count == 0)
                {
                    Console.WriteLine("ConnectionStrings is empty.");
                }
                else
                {
                    for (int i = 0; i < connectionStrings.Count; i++)
                    {
                        ConnectionStringSettings cs = connectionStrings[i];
                        Console.WriteLine("Key: {0} Value: {1}", cs.Name, cs.ConnectionString);
                    }
                }
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading connection settings");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Example_Base()
        {
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                repositoryBlog.AddRange(blogList);

                unitOfWork.Commit();
            }

            using (unitOfWorkFactory.Create())
            {
                IList<Blog> blogs = repositoryBlog.Query(filter: q => q.Rating > 5);

                long countBlogs = repositoryBlog.Count(filter: q => q.Rating > 5);

                IList<Blog> blogsWithPosts = repositoryBlog.Query(
                    filter: q => q.Url != null,                    
                    orderBy: o => o.OrderBy(x => x.Rating),
                    include: i => i.Posts
                    );

                Blog mostPopularBlog = repositoryBlog.Query(
                    callback: q => q.Where(x => x.Url == null).OrderByDescending(x => x.Rating).First());

                IList<Post> mostPopularPosts = repositoryBlog.Query(
                    callback: q => q.Where(x => x.Url == null).SelectMany( x => x.Posts).ToList());
            }
        }


        //------------------------------------------------------------------------------------------

    }
}
