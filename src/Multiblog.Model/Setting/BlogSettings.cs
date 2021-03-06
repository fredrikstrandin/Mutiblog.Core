﻿namespace Multiblog.Core.Model.Setting
{
    public class BlogSettings
    {
        public string Owner { get; set; } = "The Owner";
        public int PostsPerPage { get; set; } = 2;
        public int CommentsCloseAfterDays { get; set; } = 10;
        public bool Multitenant { get; set; } = false;
    }
}
