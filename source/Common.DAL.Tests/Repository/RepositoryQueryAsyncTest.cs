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
    public class RepositoryQueryAsyncTest
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
        public async Task Test_QueryAsync_Filter()
        {
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                // Arrange
                var repository = unitOfWork.CreateRepository<Blog>();
                repository.AddRange(blogList);


                // Act
                var result = await repository.QueryAsync(filter: q => q.Url != null);


                // Assert
                result.Should().NotBeNull("QueryAsync() вернул Null");
                result.Count.Should().Be(blogList.Count(x => x.Url != null), "QueryAsync() вернул не верное кол-во записей");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_QueryAsync_With_Tracking()
        {
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                // Arrange
                var repository = unitOfWork.CreateRepository<Blog>();
                string newName = "New_name";
                repository.Add(new Blog { Name = "Blog" });


                // Act
                var target = await repository.QueryAsync(filter: q => q.Name == "Blog", noTracking: false);
                target.First().Name = newName;

                var result = repository.GetAll().First();


                // Assert
                result.Should().NotBeNull("QueryAsync() вернул Null");
                result.Name.Should().Be(newName, "QueryAsync() вернул не привязанные к контексту записи");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_QueryAsync_With_NoTracking()
        {
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                // Arrange
                var repository = unitOfWork.CreateRepository<Blog>();
                string newName = "New_name";
                repository.Add(new Blog { Name = "Blog" });


                // Act
                var target = await repository.QueryAsync(filter: q => q.Name == "Blog", noTracking: true);
                target.First().Name = newName;

                var result = repository.GetAll().First();


                // Assert
                result.Should().NotBeNull("QueryAsync() вернул Null");
                result.Name.Should().NotBe(newName, "QueryAsync() вернул привязанные к контексту записи");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_QueryAsync_OrderBy()
        {
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                // Arrange
                var repository = unitOfWork.CreateRepository<Blog>();
                repository.AddRange(blogList);


                // Act
                var result = await repository.QueryAsync(
                    filter: q => q.Url != null,
                    orderBy:o => o.OrderBy(x => x.Rating),
                    noTracking: true
                    );


                // Assert
                result.Should().NotBeNull("QueryAsync() вернул Null");
                result.First().Rating.Should().Be(blogList.Where(x => x.Url != null).Min(x => x.Rating), "QueryAsync() не верное отсотрировал записи");
                result.Last().Rating.Should().Be(blogList.Where(x => x.Url != null).Max(x => x.Rating), "QueryAsync() не верное отсотрировал записи");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_QueryAsync_With_Include()
        {
            // Arrange
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                Blog blog = new Blog {Name = "Blog"};
                blog.Posts = new List<Post> {new Post {Title = "Title"}};
                repositoryBlog.Add(blog);
                unitOfWork.Commit();
            }


            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {

                // Act
                var result = await unitOfWork.CreateRepository<Blog>().QueryAsync(
                    filter: q => q.Name == "Blog",
                    orderBy: o => o.OrderBy(x => x.Rating),
                    include: i => i.Posts
                    );

                var title = result.First().Posts.First().Title;


                // Assert
                result.Should().NotBeNull("QueryAsync() вернул Null");
                title.Should().Be("Title", "QueryAsync() вернул не верный Post");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_QueryAsync_With_Not_Include()
        {
            // Arrange
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                Blog blog = new Blog { Name = "Blog" };
                blog.Posts = new List<Post> { new Post { Title = "Title" } };
                repositoryBlog.Add(blog);
                unitOfWork.Commit();
            }


            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {

                // Act
                var result = await unitOfWork.CreateRepository<Blog>().QueryAsync(
                    filter: q => q.Name == "Blog",
                    orderBy: o => o.OrderBy(x => x.Rating),
                    noTracking: false
                    );

                var title = result.First().Posts.First().Title;


                // Assert
                result.Should().NotBeNull("QueryAsync() вернул Null");
                title.Should().Be("Title", "QueryAsync() вернул не верный Post");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_QueryAsync_Callback_Entity()
        {
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                // Arrange
                var repository = unitOfWork.CreateRepository<Blog>();
                repository.AddRange(blogList);


                // Act
                var result = await repository.QueryAsync(
                    callback: q => q.Where(x => x.Url == null).OrderByDescending(x => x.Rating).FirstAsync());


                // Assert
                result.Should().NotBeNull("QueryAsync() вернул Null");
                result.BlogId.Should().Be(blogList.Where(x => x.Url == null).OrderByDescending(x => x.Rating).First().BlogId, "Query() вернул не верную запись");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public async Task Test_QueryAsync_Callback_Other_Entity()
        {
            using (var unitOfWork = unitOfWorkFactory.CreateAsync())
            {
                // Arrange
                var repository = unitOfWork.CreateRepository<Blog>();
                repository.AddRange(blogList);


                // Act
                var result = await repository.QueryAsync<List<int>>(
                    callback: q => q.Where(x => x.Url == null).Select(x => x.BlogId).ToListAsync());


                // Assert
                result.Should().NotBeNull("QueryAsync() вернул Null");
                result.Count().Should().Be(blogList.Count(x => x.Url == null), "QueryAsync() вернул не верной кол-во записей");
                result.First().Should().Be(blogList.Where(x => x.Url == null).Select(x => x.BlogId).First(), "QueryAsync() вернул не верную запись");
                result.Should().BeOfType<List<int>>("because a {0} is set", typeof(List<int>));

            }
        }
    }
}
