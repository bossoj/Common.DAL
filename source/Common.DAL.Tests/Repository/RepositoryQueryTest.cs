using System.Configuration;
using Common.DAL.EF;
using Common.DAL.Interface;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Common.DAL.Tests
{
    [TestClass]
    public class RepositoryQueryTest
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
        public void Test_Query_Filter()
        {
            using (unitOfWorkFactory.Create())
            {
                // Arrange
                repositoryBlog.AddRange(blogList);


                // Act
                var result = repositoryBlog.Query(filter: q => q.Url != null);


                // Assert
                result.Should().NotBeNull("Query() вернул Null");
                result.Count.Should().Be(blogList.Count(x => x.Url != null), "Query() вернул не верное кол-во записей");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_Query_With_Tracking()
        {
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                // Arrange;
                string newName = "New_name";
                repositoryBlog.Add(new Blog { Name = "Blog"});


                // Act
                repositoryBlog.Query(filter: q => q.Name == "Blog", noTracking: false).First().Name = newName;

                var result = repositoryBlog.GetAll().First();


                // Assert
                result.Should().NotBeNull("Query() вернул Null");
                result.Name.Should().Be(newName, "Query() вернул не привязанные к контексту записи");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_Query_With_NoTracking()
        {
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                // Arrange;
                string newName = "New_name";
                repositoryBlog.Add(new Blog { Name = "Blog" });


                // Act
                repositoryBlog.Query(filter: q => q.Name == "Blog", noTracking: true).First().Name = newName;

                var result = repositoryBlog.GetAll().First();


                // Assert
                result.Should().NotBeNull("Query() вернул Null");
                result.Name.Should().NotBe(newName, "Query() вернул привязанные к контексту записи");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_Query_OrderBy()
        {
            using (unitOfWorkFactory.Create())
            {
                // Arrange
                repositoryBlog.AddRange(blogList);


                // Act
                var result = repositoryBlog.Query(
                    filter: q => q.Url != null,
                    orderBy:o => o.OrderBy(x => x.Rating),
                    noTracking: true
                    );


                // Assert
                result.Should().NotBeNull("Query() вернул Null");
                result.First().Rating.Should().Be(blogList.Where(x => x.Url != null).Min(x => x.Rating), "Query() не верное отсотрировал записи");
                result.Last().Rating.Should().Be(blogList.Where(x => x.Url != null).Max(x => x.Rating), "Query() не верное отсотрировал записи");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_Query_With_Include()
        {
            // Arrange
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                Blog blog = new Blog {Name = "Blog"};
                blog.Posts = new List<Post> {new Post {Title = "Title"}};
                repositoryBlog.Add(blog);
                unitOfWork.Commit();
            }


            using (unitOfWorkFactory.Create())
            {
                // Act
                var result = repositoryBlog.Query(
                    filter: q => q.Name == "Blog",
                    orderBy: o => o.OrderBy(x => x.Rating),
                    include: i => i.Posts
                    );

                var title = result.First().Posts.First().Title;


                // Assert
                result.Should().NotBeNull("Query() вернул Null");
                title.Should().Be("Title", "Query() вернул не верный Post");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_Query_With_Not_Include()
        {
            // Arrange
            using (var unitOfWork = unitOfWorkFactory.Create())
            {                
                Blog blog = new Blog { Name = "Blog" };
                blog.Posts = new List<Post> { new Post { Title = "Title" } };
                repositoryBlog.Add(blog);
                unitOfWork.Commit();
            }


            using (unitOfWorkFactory.Create())
            {
                // Act
                var result = repositoryBlog.Query(
                    filter: q => q.Name == "Blog",
                    orderBy: o => o.OrderBy(x => x.Rating)
                    );

                var title = result.First().Posts.First().Title;


                // Assert
                result.Should().NotBeNull("Query() вернул Null");
                title.Should().Be("Title", "Query() вернул не верный Post");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_Query_Callback_Entity()
        {
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                // Arrange
                repositoryBlog.AddRange(blogList);


                // Act
                var result = repositoryBlog.Query(
                    callback: q => q.Where(x => x.Url == null).OrderByDescending(x => x.Rating).First());


                // Assert
                result.Should().NotBeNull("Query() вернул Null");
                result.BlogId.Should().Be(blogList.Where(x => x.Url == null).OrderByDescending(x => x.Rating).First().BlogId, "Query() вернул не верную запись");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_Query_Callback_Other_Entity()
        {
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                // Arrange
                repositoryBlog.AddRange(blogList);


                // Act
                var result = repositoryBlog.Query<List<int>>(
                    callback: q => q.Where(x => x.Url == null).Select(x => x.BlogId).ToList());


                // Assert
                result.Should().NotBeNull("Query() вернул Null");
                result.Count().Should().Be(blogList.Count(x => x.Url == null), "Query() вернул не верной кол-во записей");
                result.First().Should().Be(blogList.Where(x => x.Url == null).Select( x => x.BlogId).First(), "Query() вернул не верную запись");
                result.Should().BeOfType<List<int>>("because a {0} is set", typeof(List<int>));

            }
        }
    }
}
