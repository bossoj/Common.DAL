using System.Configuration;
using System.Linq;
using Common.DAL.EF;
using Common.DAL.Interface;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;

namespace Common.DAL.Tests
{
    [TestClass]
    public class UnitOfWorkAsyncTest
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
            },
            new Blog
            {
                Name = "Blog_5",
                Rating = 5,
                Url = "blog_5.ru"
            },
            new Blog
            {
                Name = "Blog_6",
                Rating = 1,
                Url = "blog_6.ru"
            },
            new Blog
            {
                Name = "Blog_7",
                Rating = 3,
                Url = "blog_7.ru"
            },
            new Blog
            {
                Name = "Blog_8",
                Rating = 8,
                Url = "blog_8.ru"
            } 
        };

        //======================================================================================

        [TestMethod]
        public void Test_UnitOfWorkAsync()
        {
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                // Arrange
                var repository = unitOfWork.CreateRepository<Blog>();
                repository.AddRange(blogList);


                // Act
                var result = repository.GetAll();


                // Assert
                result.Count.Should().Be(blogList.Count, "GetAll() вернул не верное кол-во записей");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_UnitOfWorkAsync_With_Commit()
        {
            // Arrange


            // Act
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                var repository = unitOfWork.CreateRepository<Blog>();
                repository.AddRange(blogList);
                await unitOfWork.CommitAsync();
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
        public void Test_UnitOfWorkAsync_Without_Commit()
        {
            // Arrange


            // Act
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                var repository = unitOfWork.CreateRepository<Blog>();
                repository.AddRange(blogList);
            }


            // Assert
            using ( unitOfWorkFactory.Create())
            {
                var result = repositoryBlog.GetAll();

                result.Count.Should().Be(0, "GetAll() вернул не верное кол-во записей");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_UnitOfWorkAsync_With_Await()
        {
            // Arrange


            // Act
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                var repository = unitOfWork.CreateRepository<Blog>();
                repository.AddRange(blogList);
                Task commitTask = unitOfWork.CommitAsync();

                //DoIndependentWork();

                await commitTask;
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
        public async Task Test_UnitOfWorkAsync_With_Inner_UnitOfWorkAsync()
        {
            // Arrange


            // Act
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                var repository = unitOfWork.CreateRepository<Blog>();
                repository.Add(blogList.First());

                using (var unitOfWorkInner = unitOfWorkFactory.CreateAsync())
                {
                    var repositoryInner = unitOfWorkInner.CreateRepository<Blog>();
                    repositoryInner.Add(blogList.Last());
                    await unitOfWorkInner.CommitAsync();
                }

                await unitOfWork.CommitAsync();
            }


            // Assert
            using (unitOfWorkFactory.Create())
            {
                var result = repositoryBlog.GetAll();

                result.Count.Should().Be(2, "GetAll() вернул не верное кол-во записей");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_UnitOfWorkAsync_With_Inner_UnitOfWorkAsync_With_Rollback()
        {
            // Arrange


            // Act
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                var repository = unitOfWork.CreateRepository<Blog>();
                repository.Add(blogList.First());

                using (var unitOfWorkInner = unitOfWorkFactory.CreateAsync())
                {
                    var repositoryInner = unitOfWorkInner.CreateRepository<Blog>();
                    repositoryInner.Add(blogList.Last());

                }  //Not Transaction Rollback(), Not Flush()

                await unitOfWork.CommitAsync();                
            }


            // Assert
            using (unitOfWorkFactory.Create())
            {
                var result = repositoryBlog.GetAll();

                result.Count.Should().Be(2, "GetAll() вернул не верное кол-во записей");
            }
        }

    }
}
