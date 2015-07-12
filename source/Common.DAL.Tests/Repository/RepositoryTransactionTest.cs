using Common.DAL.EF;
using Common.DAL.Interface;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Common.DAL.Tests
{
    [TestClass]
    public class RepositoryTransactionTest
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

        private List<Blog> blogList = new List<Blog>
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
            } ,
            new Blog
            {
                Name = "Blog_5",
                Rating = 1
            },
            new Blog
            {
                Name = "Blog_6",
                Rating = 5
            }
        };

        //======================================================================================

        [TestMethod]
        public void Test_Transaction_With_Complete_With_UnitOfWork_Commit()
        {
            // Arrange
            using (var scope = new TransactionScope())
            {
                using (var unitOfWork = unitOfWorkFactory.Create())
                {
                    repositoryBlog.AddRange(blogList);
                    unitOfWork.Commit();
                }

                scope.Complete();
            }

            using (unitOfWorkFactory.Create())
            {
                // Act
                var result = repositoryBlog.Count();

                // Assert
                result.Should().Be(blogList.LongCount(), "Count() вернул не верное кол-во записей");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_Transaction_With_Complete_Without_UnitOfWork_Commit()
        {
            // Arrange
            using (var scope = new TransactionScope())
            {
                using (unitOfWorkFactory.Create())
                {
                    repositoryBlog.AddRange(blogList);
                }

                scope.Complete();
            }

            using (unitOfWorkFactory.Create())
            {
                // Act
                var result = repositoryBlog.Count();

                // Assert
                result.Should().Be(blogList.LongCount(), "Count() вернул не верное кол-во записей");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_Transaction_Without_Complete()
        {
            // Arrange
            using (new TransactionScope())
            {
                using (var unitOfWork = unitOfWorkFactory.Create())
                {
                    repositoryBlog.AddRange(blogList);
                    unitOfWork.Commit();
                }
            }

            using (unitOfWorkFactory.Create())
            {
                // Act
                var result = repositoryBlog.Count();

                // Assert
                result.Should().Be(0, "Count() вернул не верное кол-во записей");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_Async_Transaction_With_Complete_With_UnitOfWork_Commit()
        {
            // Arrange
            using (var scope = new TransactionScope())
            {
                using (var unitOfWork = unitOfWorkFactory.CreateAsync())
                {
                    var repository = unitOfWork.CreateRepository<Blog>();
                    repository.AddRange(blogList);
                    await unitOfWork.CommitAsync();
                }

                scope.Complete();
            }

            using (unitOfWorkFactory.Create())
            {
                // Act
                var result = repositoryBlog.Count();

                // Assert
                result.Should().Be(blogList.LongCount(), "Count() вернул не верное кол-во записей");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_Async_Transaction_Without_Complete()
        {
            // Arrange
            using (new TransactionScope())
            {
                using (var unitOfWork = unitOfWorkFactory.CreateAsync())
                {
                    var repository = unitOfWork.CreateRepository<Blog>();
                    repository.AddRange(blogList);
                    await unitOfWork.CommitAsync();
                }
            }

            using (unitOfWorkFactory.Create())
            {
                // Act
                var result = repositoryBlog.Count();

                // Assert
                result.Should().Be(0, "Count() вернул не верное кол-во записей");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_Async_Transaction_With_Complete_Without_UnitOfWork_Commit()
        {
            // Arrange
            using (var scope = new TransactionScope())
            {
                using (var unitOfWork = unitOfWorkFactory.CreateAsync())
                {
                    var repository = unitOfWork.CreateRepository<Blog>();
                    repository.AddRange(blogList);
                }

                scope.Complete();
            }

            using (unitOfWorkFactory.Create())
            {
                // Act
                var result = repositoryBlog.Count();

                // Assert
                result.Should().Be(blogList.LongCount(), "Count() вернул не верное кол-во записей");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_Transaction_UnitOfWork_With_Inner_UnitOfWork()
        {
            // Arrange


            // Act
            using (var scope = new TransactionScope())
            {
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

                scope.Complete();
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
        public void Test_Transaction_UnitOfWork_With_Inner_UnitOfWork_Without_Complete()
        {
            // Arrange


            // Act
            using (new TransactionScope())
            {
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
            }

            // Assert
            using (unitOfWorkFactory.Create())
            {
                var result = repositoryBlog.GetAll();

                result.Count.Should().Be(0, "GetAll() вернул не верное кол-во записей");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_Async_Transaction_UnitOfWorkAsync_With_Inner_UnitOfWorkAsync()
        {
            // Arrange


            // Act
            using (var scope = new TransactionScope())
            {
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

                scope.Complete();
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
        public async Task Test_Async_Transaction_UnitOfWorkAsync_With_Inner_UnitOfWorkAsync_Without_Complete()
        {
            // Arrange


            // Act
            using (new TransactionScope())
            {
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
            }

            // Assert
            using (unitOfWorkFactory.Create())
            {
                var result = repositoryBlog.GetAll();

                result.Count.Should().Be(0, "GetAll() вернул не верное кол-во записей");
            }
        }
    }
}