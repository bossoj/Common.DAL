using System.Configuration;
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
    public class UnitOfWorkTest
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
            using (var uow = unitOfWorkFactory.Create())
            {
                repositoryBlog.DeleteRange(repositoryBlog.GetAll());
                repositoryPost.DeleteRange(repositoryPost.GetAll());

                uow.Commit();
            }
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
        public void Test_UnitOfWork()
        {
            using (unitOfWorkFactory.Create())
            {
                // Arrange
                repositoryBlog.AddRange(blogList);


                // Act
                var result = repositoryBlog.GetAll();


                // Assert
                result.Should().NotBeNull("GetAll() вернул Null");
                result.Count.Should().Be(blogList.Count, "GetAll() вернул не верное кол-во записей");
            }
        }


        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_UnitOfWork_With_Commit()
        {
            // Arrange


            // Act
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                repositoryBlog.AddRange(blogList);
                unitOfWork.Commit();
            }


            // Assert
            using (unitOfWorkFactory.Create())
            {
                var result = repositoryBlog.GetAll();

                result.Count.Should().Be(blogList.Count, "GetAll() вернул не верное кол-во записей");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_UnitOfWork_Without_Commit()
        {
            // Arrange


            // Act
            using (unitOfWorkFactory.Create())
            {
                repositoryBlog.AddRange(blogList);
            } // Not Commot


            // Assert
            using (unitOfWorkFactory.Create())
            {
                var result = repositoryBlog.GetAll();

                result.Count.Should().Be(0, "GetAll() вернул не верное кол-во записей");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_UnitOfWork_With_Inner_UnitOfWork()
        {
            // Arrange


            // Act
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                repositoryBlog.AddRange(blogList);

                using (var unitOfWorkInner = unitOfWorkFactory.Create())
                {
                    repositoryBlog.AddRange(blogList);
                    unitOfWorkInner.Commit(); //Not Transaction Commit(), only Flush()
                }

                unitOfWork.Commit(); //Transaction Commit()
            }


            // Assert
            using (unitOfWorkFactory.Create())
            {
                var result = repositoryBlog.GetAll();

                result.Count.Should().Be(blogList.Count * 2, "GetAll() вернул не верное кол-во записей");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_UnitOfWork_With_Inner_UnitOfWork_With_Rollback()
        {
            // Arrange


            // Act
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                repositoryBlog.AddRange(blogList);

                using (unitOfWorkFactory.Create())
                {
                    repositoryBlog.AddRange(blogList);
                } //Not Transaction Rollback(), Not Flush()

                unitOfWork.Commit();
            }


            // Assert
            using (unitOfWorkFactory.Create())
            {
                var result = repositoryBlog.GetAll();

                result.Count.Should().Be(blogList.Count * 2, "GetAll() вернул не верное кол-во записей");
            }
        }
    }
}
