using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Data.Entity;
using Common.Entity;

namespace Common.DAL.Tests
{
    public class BlogContext : DbContext
    {
        public BlogContext() 
            : base() { }

        public BlogContext(string connectionString) 
            : base(connectionString) { }

        public BlogContext(DbConnection existingConnection, bool contextOwnsConnection = false) 
            : base(existingConnection, contextOwnsConnection) { }

        public DbSet<Blog> Blogs { get; set; }

        public DbSet<Post> Posts { get; set; }
    }

    public class Blog : IEntity
    {
        [Key]
        public int BlogId { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public int Rating { get; set; }

        public virtual List<Post> Posts { get; set; }
    }

    public class Post : IEntity
    {
        [Key]
        public int PostId { get; set; }

        [MaxLength(200)]
        public string Title { get; set; }

        public string Content { get; set; }

        public int BlogId { get; set; }

        public Blog Blog { get; set; }

        public string Abstract { get; set; }
    }
}
