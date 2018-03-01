﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Multiblog.Core.Models;

namespace Multiblog.Core.Services
{
    public interface IBlogService
    {
        Task<IEnumerable<Post>> GetPostsAsync(string blogId, int count, int skip = 0);
        Task<IEnumerable<Post>> GetPostsByCategory(string blogId, string category, int count, int skip = 0);
        Task<Post> GetPostBySlug(string blogId, string slug);
        Task<Post> GetPostById(string blogId, string id);
        Task<IEnumerable<string>> GetCategoryAsync(string blogId);
        Task<string> SavePost(Post post);
        Task DeletePost(Post post);
        Task<string> SaveFile(byte[] bytes, string fileName, string suffix = null);
        Task AddCommentAsync(string iD, Comment comment);
        Task UpdatePostAsync(Post existing);
    }

    public abstract class InMemoryBlogServiceBase //: IBlogService
    {
        public InMemoryBlogServiceBase(IHttpContextAccessor contextAccessor)
        {
            ContextAccessor = contextAccessor;
        }

        protected List<Post> Cache { get; set; }
        protected IHttpContextAccessor ContextAccessor { get; }

        public virtual Task<IEnumerable<Post>> GetPosts(int count, int skip = 0)
        {
            bool isAdmin = IsAdmin();

            var posts = Cache
                .Where(p => p.PubDate <= DateTime.UtcNow && (p.Status == Status.Publish || isAdmin))
                .Skip(skip)
                .Take(count);

            return Task.FromResult(posts);
        }

        public virtual Task<IEnumerable<Post>> GetPostsByCategory(string category)
        {
            bool isAdmin = IsAdmin();

            var posts = from p in Cache
                        where p.PubDate <= DateTime.UtcNow && (p.Status == Status.Publish || isAdmin)
                        where p.Categories.Contains(category, StringComparer.OrdinalIgnoreCase)
                        select p;

            return Task.FromResult(posts);

        }

        public virtual Task<Post> GetPostBySlug(string slug)
        {
            var post = Cache.FirstOrDefault(p => p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
            bool isAdmin = IsAdmin();

            if (post != null && post.PubDate <= DateTime.UtcNow && (post.Status == Status.Publish || isAdmin))
            {
                return Task.FromResult(post);
            }

            return Task.FromResult<Post>(null);
        }

        public virtual Task<Post> GetPostById(string id)
        {
            var post = Cache.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            bool isAdmin = IsAdmin();

            if (post != null && post.PubDate <= DateTime.UtcNow && (post.Status == Status.Publish || isAdmin))
            {
                return Task.FromResult(post);
            }

            return Task.FromResult<Post>(null);
        }

        public virtual Task<IEnumerable<string>> GetCategories()
        {
            bool isAdmin = IsAdmin();

            var categories = Cache
                .Where(p => p.Status == Status.Publish || isAdmin)
                .SelectMany(post => post.Categories)
                .Select(cat => cat.ToLowerInvariant())
                .Distinct();

            return Task.FromResult(categories);
        }

        public abstract Task SavePost(Post post);

        public abstract Task DeletePost(Post post);

        public abstract Task<string> SaveFile(byte[] bytes, string fileName, string suffix = null);

        protected void SortCache()
        {
            Cache.Sort((p1, p2) => p2.PubDate.CompareTo(p1.PubDate));
        }

        protected bool IsAdmin()
        {
            return ContextAccessor.HttpContext?.User?.Identity.IsAuthenticated == true;
        }

        public Task AddCommentAsync(string iD, Comment comment)
        {
            throw new NotImplementedException();
        }

        public Task UpdatePostAsync(Post existing)
        {
            throw new NotImplementedException();
        }
    }
}