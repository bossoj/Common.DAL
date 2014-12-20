Common Data Access Layer
==========

Generic interface and implementation (Entity Framework) of the DAL (repository pattern, unit of work pattern) in C# .NET


Installation
------------
Download sources.

License
------------
[Apache License, Version 2.0](http://www.apache.org/licenses/LICENSE-2.0)

Example
------------

### Initialization

```csharp
string connectionString = "...";

dbContextFactory = new DbContextFactory(connectionString, s => new BlogContext(s));
dbContextProvider = new ThreadDbContextProvider();

unitOfWorkFactory = new UnitOfWorkFactory(dbContextFactory, dbContextProvider);
repositoryBlog = new Repository<Blog>(dbContextProvider);
```


```csharp
List<Blog> blogList = new List<Blog>
{
    new Blog
    {
        Name = "Blog_1",
        Rating = 8,
        Url = "blog_1.ru"
    },
    new Blog
    {
        Name = "Blog_2",
        Rating = 1,
        Url = "blog_2.ru"
    }
};
```

### Basic 

```csharp
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
        noTracking: true,
        orderBy: o => o.OrderBy(x => x.Rating),
        include: i => i.Posts
        );
}
```

### Repository


### Async Repository


### Query


### PagedList


### Unit Of Work


### Async Unit Of Work


### Nested Unit Of Work


### Nested Async Unit Of Work


### Transaction
*  **Use new transaction (default)**


*  **Use current transaction**

