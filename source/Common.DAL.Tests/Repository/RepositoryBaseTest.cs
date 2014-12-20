using System.Configuration;
using Common.DAL.EF;
using Common.DAL.Interface;
using FluentAssertions;
using FluentAssertions.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Common.DAL.Tests
{
    [TestClass]
    public class RepositoryBaseTest
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

        readonly List<Blog> blogList = new List<Blog>
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
        public void Test_GetAll()
        {
            using (unitOfWorkFactory.Create())
            {
                // Arrange
                repositoryBlog.AddRange(blogList);


                // Act
                var result = repositoryBlog.GetAll();


                // Assert
                result.Should().NotBeNull("GetAll() вернул Null");
                result.Should().BeOfType<List<Blog>>("because a {0} is set", typeof(List<Blog>));
                result.Count.Should().Be(blogList.Count, "GetAll() вернул не верное кол-во записей");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_GetAll_With_Tracking()
        {
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                // Arrange;
                string newName = "New_name";
                repositoryBlog.Add(new Blog { Name = "Blog"});


                // Act
                repositoryBlog.GetAll().First().Name = newName;

                var result = repositoryBlog.GetAll().First();


                // Assert
                result.Should().NotBeNull("GetAll() вернул Null");
                result.Name.Should().Be( newName, "GetAll() вернул не привязанные к контексту записи");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_GetAll_With_NoTracking()
        {
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                // Arrange;
                string newName = "New_name";
                repositoryBlog.Add(new Blog { Name = "Blog" });


                // Act
                repositoryBlog.GetAll(noTracking: true).First().Name = newName;

                var result = repositoryBlog.GetAll().First();


                // Assert
                result.Should().NotBeNull("GetAll() вернул Null");
                result.Name.Should().NotBe(newName, "GetAll() вернул привязанные к контексту записи");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_Find()
        {
            using (unitOfWorkFactory.Create())
            {
                // Arrange
                Blog blog = new Blog {Name = "Blog"};
                repositoryBlog.Add(blog);


                // Act
                var result = repositoryBlog.Find(blog.BlogId);


                // Assert
                result.Should().NotBeNull("Find() вернул Null");
                result.Should().BeOfType<Blog>("because a {0} is set", typeof(Blog));
                result.BlogId.Should().Be(blog.BlogId, "Find() вернул не верный объект");
            }
        }
        
        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_Find_Not_Found()
        {
            using (unitOfWorkFactory.Create())
            {
                // Arrange


                // Act
                var result = repositoryBlog.Find(1);


                // Assert
                result.Should().BeNull("Find() вернул не Null");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_Add()
        {
            // Arrange
            Blog blog = new Blog { Name = "Blog" };


            // Act
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                repositoryBlog.Add(blog);
                unitOfWork.Commit();
            }


            // Assert
            using (unitOfWorkFactory.Create())
            {
                var result = repositoryBlog.GetAll();

                result.Count.Should().Be(1, "GetAll() вернул не верное кол-во записей");
                result.Last().Name.Should().Be("Blog", "GetAll() вернул не верный Blog");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_AddRange()
        {
            // Arrange
            Blog blog = new Blog { Name = "Blog" };
            Blog blog2 = new Blog { Name = "Blog2" };


            // Act
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                repositoryBlog.AddRange(new [] { blog, blog2});
                unitOfWork.Commit();
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
        public void Test_AddOrUpdate_New_Entity()
        {
            // Arrange
            Blog blog = new Blog { Name = "Blog" };


            // Act
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                repositoryBlog.AddOrUpdate(blog);
                unitOfWork.Commit();
            }


            // Assert
            using (unitOfWorkFactory.Create())
            {
                var result = repositoryBlog.GetAll();

                result.Count.Should().Be(1, "GetAll() вернул не верное кол-во записей");
                result.Last().Name.Should().Be("Blog", "GetAll() вернул не верный Blog");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_AddOrUpdate_Exists_Entity()
        {
            using (unitOfWorkFactory.Create())
            {
                // Arrange
                string newName = "New_name";
                Blog newBlog = new Blog { Name = "Blog" };
                repositoryBlog.Add(newBlog);


                // Act
                var blog = repositoryBlog.GetAll().First();
                blog.Name = newName;

                var count = repositoryBlog.AddOrUpdate(blog);

                var result = repositoryBlog.GetAll().First();


                // Assert
                count.Should().Be(1, "AddOrUpdate() вернул не верное количество измененных/сохраненных объектов");
                result.Name.Should().Be(newName, "AddOrUpdate() не обновил объект");
                newBlog.Name.Should().Be(newName, "AddOrUpdate() не обновил объект в сессии");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_AddOrUpdate_Range()
        {
            // Arrange
            Blog blog = new Blog { Name = "Blog" };
            Blog blog2 = new Blog { Name = "Blog2" };


            // Act
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                repositoryBlog.AddOrUpdate(new[] { blog, blog2 });
                unitOfWork.Commit();
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
        public void Test_AddOrUpdate_With_Identifier_Add()
        {
            using (unitOfWorkFactory.Create())
            {
                // Arrange
                string newName = "New_name";
                Blog newBlog = new Blog { Name = "Blog" };
                repositoryBlog.Add(newBlog);


                // Act
                var blog = repositoryBlog.GetAll().First();
                blog.Name = newName;

                var count = repositoryBlog.AddOrUpdate(blog, b => b.Name);

                var result = repositoryBlog.GetAll();


                // Assert
                count.Should().Be(1, "AddOrUpdate() вернул не верное количество измененных/сохраненных объектов");
                result.Count.Should().Be(2, "AddOrUpdate() не добавил объект");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_AddOrUpdate_With_Identifier_Update()
        {
            using (unitOfWorkFactory.Create())
            {
                // Arrange
                string newName = "New_name";
                Blog newBlog = new Blog { Name = "Blog" };
                repositoryBlog.Add(newBlog);


                // Act
                var blog = repositoryBlog.GetAll().First();
                blog.Name = newName;

                var count = repositoryBlog.AddOrUpdate(blog, b => b.BlogId);

                var result = repositoryBlog.GetAll();


                // Assert
                count.Should().Be(1, "AddOrUpdate() вернул не верное количество измененных/сохраненных объектов");
                result.Count.Should().Be(1, "AddOrUpdate() не добавил объект");
                result.First().Name.Should().Be(newName, "AddOrUpdate() не обновил объект");
                newBlog.Name.Should().Be(newName, "AddOrUpdate() не обновил объект в сессии");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_Update()
        {
            using (unitOfWorkFactory.Create())
            {
                // Arrange
                string newName = "New_name";
                Blog newBlog = new Blog { Name = "Blog" };
                repositoryBlog.Add(newBlog);


                // Act
                var blog = repositoryBlog.GetAll().First();
                blog.Name = newName;

                var count = repositoryBlog.Update(blog);

                var result = repositoryBlog.GetAll().First();


                // Assert
                count.Should().Be(1, "Update() вернул не верное количество измененных/сохраненных объектов");
                result.Name.Should().Be(newName, "Update() не обновил объект");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_Update_New_Entity()
        {
            using (unitOfWorkFactory.Create())
            {
                // Arrange
                Blog blog = new Blog { Name = "Blog" };


                // Act
                Action act = () => repositoryBlog.Update(blog); 


                // Assert
                act.ShouldThrow<DbUpdateConcurrencyException>("Не было вызвано исключение о том, что невозможно обновить запись");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_Update_NoTracking_Entity()
        {
            using (unitOfWorkFactory.Create())
            {
                // Arrange
                string newName = "New_name";
                Blog newBlog = new Blog { Name = "Blog" };
                repositoryBlog.Add(newBlog);


                // Act
                var blog = repositoryBlog.GetAll(noTracking: true).First();
                blog.Name = newName;

                Action act = () => repositoryBlog.Update(blog);


                // Assert
                act.ShouldThrow<InvalidOperationException>("Не было вызвано исключение о том, что невозможно обновить запись");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_Delete()
        {
            // Arrange
            Blog blog = new Blog { Name = "Blog" };

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                repositoryBlog.Add(blog);
                unitOfWork.Commit();
            }


            using (unitOfWorkFactory.Create())
            {
                // Act
                var count = repositoryBlog.Delete(blog);

                var result = repositoryBlog.GetAll();


                // Assert
                count.Should().Be(1, "Delete() вернул не верное количество удаленных объектов");
                result.Count.Should().Be(0, "Delete() не удалил объект");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_Delete_New_Entity()
        {
            using (unitOfWorkFactory.Create())
            {
                // Arrange
                Blog blog = new Blog { Name = "Blog" };


                // Act
                Action act = () => repositoryBlog.Delete(blog);


                // Assert
                act.ShouldThrow<DbUpdateConcurrencyException>("Не было вызвано исключение о том, что невозможно удалить запись");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_DeleteRange()
        {
            using (unitOfWorkFactory.Create())
            {
                // Arrange
                Blog blog = new Blog {Name = "Blog"};
                Blog blog2 = new Blog {Name = "Blog2"};

                repositoryBlog.AddRange(new[] {blog, blog2});


                // Act
                var count = repositoryBlog.DeleteRange(new[] { blog, blog2 });

                var result = repositoryBlog.GetAll();


                // Assert
                count.Should().Be(2, "DeleteRange() вернул не верное количество удаленных объектов");
                result.Count.Should().Be(0, "DeleteRange() не удалил объект");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_DeleteAll()
        {
            using (unitOfWorkFactory.Create())
            {
                // Arrange
                Blog blog = new Blog { Name = "Blog", Rating = 1 };
                Blog blog2 = new Blog { Name = "Blog2", Rating = 1 };
                Blog blog3 = new Blog { Name = "Blog3", Rating = 2 };

                repositoryBlog.AddRange(new[] { blog, blog2, blog3 });


                // Act
                var count = repositoryBlog.DeleteAll( x => x.Rating == 1);

                var result = repositoryBlog.GetAll();


                // Assert
                count.Should().Be(2, "DeleteAll() вернул не верное количество удаленных объектов");
                result.Count.Should().Be(1, "DeleteAll() удалил не верное количество объектов");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_Count()
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
            using (unitOfWorkFactory.Create())
            {
                var result = repositoryBlog.Count();

                result.Should().Be(2, "Count() вернул не верное кол-во записей");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_Count_With_Filter()
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
            using (unitOfWorkFactory.Create())
            {
                var result = repositoryBlog.Count(x => x.Rating == 1);

                result.Should().Be(2, "Count(filter) вернул не верное кол-во записей");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_Paged()
        {
            using (unitOfWorkFactory.Create())
            {
                // Arrange
                repositoryBlog.AddRange(blogList);
                int pageNumber = 2;
                int pageSize = 2;


                // Act
                var result = repositoryBlog.Paged(
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    filter: null,
                    orderBy: o => o.OrderBy(x => x.BlogId)
                    );


                // Assert
                result.Should().NotBeNull("Paged() вернул Null");
                result.Count.Should().Be(pageSize, "Paged() вернул не верное кол-во записей");
                result.First().BlogId.Should().Be(blogList.OrderBy(x => x.BlogId).Skip((pageNumber - 1) * pageSize).Take(pageSize).First().BlogId,
                    "Paged() вернул не верную запись");

            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_DeleteImmediately()
        {
            using (unitOfWorkFactory.Create())
            {
                // Arrange
                Blog blog = new Blog { Name = "Blog", Rating = 1 };
                Blog blog2 = new Blog { Name = "Blog2", Rating = 1 };
                Blog blog3 = new Blog { Name = "Blog3", Rating = 2 };

                repositoryBlog.AddRange(new[] { blog, blog2, blog3 });


                // Act
                var count = repositoryBlog.DeleteImmediately(x => x.Rating == 1);

                var result = repositoryBlog.GetAll();


                // Assert
                count.Should().Be(2, "DeleteImmediately() вернул не верное количество удаленных объектов");
                result.Count.Should().Be(1, "DeleteImmediately() удалил не верное количество объектов");
            }
        }

        //------------------------------------------------------------------------------------------

        [TestMethod]
        public void Test_UpdateImmediately()
        {
            using (unitOfWorkFactory.Create())
            {
                // Arrange
                Blog blog = new Blog { Name = "Blog", Rating = 1 };
                Blog blog2 = new Blog { Name = "Blog2", Rating = 1 };
                Blog blog3 = new Blog { Name = "Blog3", Rating = 2 };

                repositoryBlog.AddRange(new[] { blog, blog2, blog3 });


                // Act
                var count = repositoryBlog.UpdateImmediately(x => x.Rating == 1, b => new Blog { Rating = 2 });

                var result = repositoryBlog.Query( x => x.Rating == 2);


                // Assert
                count.Should().Be(2, "UpdateImmediately() вернул не верное количество обновленных объектов");
                result.Count.Should().Be(3, "UpdateImmediately() обновил не верное количество объектов");
            }
        }
    }
}
