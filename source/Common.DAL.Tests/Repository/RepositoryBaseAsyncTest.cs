using System.Configuration;
using Common.DAL.EF;
using Common.DAL.Interface;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace Common.DAL.Tests
{
    [TestClass]
    public class RepositoryBaseAsyncTest
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
        public async Task Test_GetAllAsync()
        {
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                // Arrange
                var repository = unitOfWork.CreateRepository<Blog>();
                repository.AddRange(blogList);


                // Act
                var result = await repository.GetAllAsync();


                // Assert
                result.Should().NotBeNull("GetAllAsync() вернул Null");
                result.Should().BeOfType<List<Blog>>("because a {0} is set", typeof(List<Blog>));
                result.Count.Should().Be(blogList.Count, "GetAllAsync() вернул не верное кол-во записей");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_FindAsync()
        {
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                // Arrange
                var repository = unitOfWork.CreateRepository<Blog>();
                Blog blog = new Blog { Name = "Blog" };
                repository.Add(blog);


                // Act
                var result = await repository.FindAsync(blog.BlogId);


                // Assert
                result.Should().NotBeNull("FindAsync() вернул Null");
                result.Should().BeOfType<Blog>("because a {0} is set", typeof(Blog));
                result.BlogId.Should().Be(blog.BlogId, "FindAsync() вернул не верный объект");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_FindAsync_Not_Found()
        {
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                // Arrange
                var repository = unitOfWork.CreateRepository<Blog>();


                // Act
                var result = await repository.FindAsync(1);


                // Assert
                result.Should().BeNull("FindAsync() вернул не Null");

            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_AddRangeAsync()
        {
            // Arrange
            Blog blog = new Blog { Name = "Blog" };
            Blog blog2 = new Blog { Name = "Blog2" };


            // Act
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                await unitOfWork.CreateRepository<Blog>().AddRangeAsync(new[] { blog, blog2 });
                await unitOfWork.CommitAsync();
            }


            // Assert
            using (unitOfWorkFactory.Create())
            {
                var result = repositoryBlog.GetAll();

                result.Count.Should().Be(2, "GetAll() вернул не верное кол-во записей");
                result.First().Name.Should().Be("Blog", "GetAll() вернул не верный Blog");
                result.Last().Name.Should().Be("Blog2", "GetAll() вернул не верный Blog");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_AddOrUpdateAsync_New_Entity()
        {
            // Arrange
            Blog blog = new Blog { Name = "Blog" };
            Blog blog2 = new Blog { Name = "Blog2" };


            // Act
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                await unitOfWork.CreateRepository<Blog>().AddOrUpdateAsync(new [] { blog, blog2 });
                await unitOfWork.CommitAsync();
            }


            // Assert
            using (unitOfWorkFactory.Create())
            {
                var result = repositoryBlog.GetAll();

                result.Count.Should().Be(2, "GetAll() вернул не верное кол-во записей");
                result.First().Name.Should().Be("Blog", "GetAll() вернул не верный Blog");
                result.Last().Name.Should().Be("Blog2", "GetAll() вернул не верный Blog");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_AddOrUpdateAsync_Exists_Entity()
        {
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                // Arrange
                var repository = unitOfWork.CreateRepository<Blog>();
                string newName = "New_name";
                Blog newBlog = new Blog { Name = "Blog" };
                Blog newBlog2 = new Blog { Name = "Blog2" };
                repository.Add(newBlog);


                // Act
                var blog = repository.GetAll().First();
                blog.Name = newName;

                var count = await repository.AddOrUpdateAsync(new[] { blog, newBlog2 });

                var result = repository.GetAll().First();


                // Assert
                count.Should().Be(2, "AddOrUpdateAsync() вернул не верное количество измененных/сохраненных объектов");
                result.Name.Should().Be(newName, "AddOrUpdateAsync() не обновил объект");
                newBlog.Name.Should().Be(newName, "AddOrUpdateAsync() не обновил объект в сессии");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_DeleteAllAsync()
        {
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                // Arrange
                var repository = unitOfWork.CreateRepository<Blog>();
                Blog blog = new Blog { Name = "Blog", Rating = 1 };
                Blog blog2 = new Blog { Name = "Blog2", Rating = 1 };
                Blog blog3 = new Blog { Name = "Blog3", Rating = 2 };
                repository.AddRange(new[] { blog, blog2, blog3 });


                // Act
                var count = await repository.DeleteAllAsync(x => x.Rating == 1);

                var result = repository.GetAll();


                // Assert
                count.Should().Be(2, "DeleteAllAsync() вернул не верное количество удаленных объектов");
                result.Count.Should().Be(1, "DeleteAllAsync() удалил не верное количество объектов");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_CountAsync()
        {
            // Arrange
            Blog blog = new Blog { Name = "Blog" };
            Blog blog2 = new Blog { Name = "Blog2" };


            // Act
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                repositoryBlog.AddRange(new[] { blog, blog2 });
                unitOfWork.Commit();
            }


            // Assert
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                var result = await unitOfWork.CreateRepository<Blog>().CountAsync();

                result.Should().Be(2, "Count() вернул не верное кол-во записей");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_Count_With_Filter()
        {
            // Arrange
            Blog blog = new Blog { Name = "Blog", Rating = 1 };
            Blog blog2 = new Blog { Name = "Blog2", Rating = 1 };
            Blog blog3 = new Blog { Name = "Blog3", Rating = 2 };


            // Act
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                repositoryBlog.AddRange(new[] { blog, blog2, blog3 });
                unitOfWork.Commit();
            }


            // Assert
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                var result = await unitOfWork.CreateRepository<Blog>().CountAsync(x => x.Rating == 1);

                result.Should().Be(2, "Count(filter) вернул не верное кол-во записей");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_PagedAsync()
        {
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                // Arrange
                var repository = unitOfWork.CreateRepository<Blog>();
                repository.AddRange(blogList);
                int pageNumber = 2;
                int pageSize = 2;


                // Act
                var result = await repository.PagedAsync(
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    filter: null,
                    orderBy: o => o.OrderBy(x => x.BlogId)
                    );


                // Assert
                result.Should().NotBeNull("PagedAsync() вернул Null");
                result.Count.Should().Be(pageSize, "PagedAsync() вернул не верное кол-во записей");
                result.First().BlogId.Should().Be(blogList.OrderBy(x => x.BlogId).Skip((pageNumber - 1) * pageSize).Take(pageSize).First().BlogId,
                    "PagedAsync() вернул не верную запись");

            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_DeleteImmediatelyAsync()
        {
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                // Arrange
                var repository = unitOfWork.CreateRepository<Blog>();
                Blog blog = new Blog { Name = "Blog", Rating = 1 };
                Blog blog2 = new Blog { Name = "Blog2", Rating = 1 };
                Blog blog3 = new Blog { Name = "Blog3", Rating = 2 };

                repository.AddRange(new[] { blog, blog2, blog3 });


                // Act
                var count = await repository.DeleteImmediatelyAsync(x => x.Rating == 1);

                var result = repository.GetAll();


                // Assert
                count.Should().Be(2, "DeleteImmediatelyAsync() вернул не верное количество удаленных объектов");
                result.Count.Should().Be(1, "DeleteImmediatelyAsync() удалил не верное количество объектов");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_UpdateImmediatelyAsync()
        {
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                // Arrange
                var repository = unitOfWork.CreateRepository<Blog>();
                Blog blog = new Blog { Name = "Blog", Rating = 1 };
                Blog blog2 = new Blog { Name = "Blog2", Rating = 1 };
                Blog blog3 = new Blog { Name = "Blog3", Rating = 2 };

                repository.AddRange(new[] { blog, blog2, blog3 });


                // Act
                var count = await repository.UpdateImmediatelyAsync(x => x.Rating == 1, b => new Blog { Rating = 2 });

                var result = repository.Query(x => x.Rating == 2);


                // Assert
                count.Should().Be(2, "UpdateImmediatelyAsync() вернул не верное количество обновленных объектов");
                result.Count.Should().Be(3, "UpdateImmediatelyAsync() обновил не верное количество объектов");
            }
        }
    }
}
