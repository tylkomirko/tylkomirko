﻿using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WykopSDK.API.Models.Converters;

namespace WykopSDK.API.Models
{
    public enum UserSex
    {
        Male,
        Female,
        None
    };

    public enum UserGroup
    {
        Green,
        Orange,
        Maroon,
        Admin,
        Banned,
        Deleted,
        Client
    };

    [ImplementPropertyChanged]
    public class EntryBase
    {
        [JsonProperty("id")]
        public uint ID { get; set; }
        [JsonProperty("date")]
        public DateTime Date { get; set; }
        [JsonProperty("blocked")]
        public bool Blacklisted { get; set; }
        [JsonProperty("deleted")]
        public bool Deleted { get; set; }

        [JsonProperty("author")]
        public string AuthorName { get; set; }
        [JsonProperty("author_avatar_big")]
        public string AuthorAvatarURL { get; set; }
        [JsonProperty("author_group")]
        [JsonConverter(typeof(GroupConverter))]
        public UserGroup AuthorGroup { get; set; }
        [JsonProperty("author_sex")]
        [JsonConverter(typeof(SexEnumConverter))]
        public UserSex AuthorSex { get; set; }

        [JsonProperty("body")]
        public string Text { get; set; }
        [JsonProperty("embed")]
        public Embed Embed { get; set; }

        [JsonProperty("vote_count")]
        public uint VoteCount { get; set; }
        [JsonProperty("voters")]
        [JsonConverter(typeof(VotersConverter))]
        public ObservableCollection<string> Voters { get; set; }
        [JsonProperty("user_vote")]
        [JsonConverter(typeof(BoolConverter))]
        public bool Voted { get; set; }
    }

    [ImplementPropertyChanged]
    public class Entry : EntryBase
    {
        [JsonProperty("comment_count")]
        public uint CommentCount { get; set; }
        [JsonProperty("comments")]
        public List<EntryComment> Comments { get; set; }

        [JsonProperty("user_favorite")]
        public bool Favourite { get; set; }
        [JsonProperty("can_comment")]
        public bool? CanComment { get; set; }

        /* Removed properties:
         public string author_avatar_big { get; set; }
         public string author_avatar_med { get; set; }
         public string author_avatar_lo { get; set; }
         public object source { get; set; }
         public string url { get; set; }
         public string receiver { get; set; }
         public string receiver_avatar { get; set; }
         public string receiver_avatar_big { get; set; }
         public string receiver_avatar_med { get; set; }
         public string receiver_avatar_lo { get; set; }
         public int? receiver_group { get; set; }
         public object receiver_sex { get; set; }
         public string type { get; set; }
         public string violation_url { get; set; }
         public string app { get; set; } 
         * */
    }

    [ImplementPropertyChanged]
    public class EntryComment : EntryBase
    {
        [JsonProperty("entry_id")]
        public uint EntryID { get; set; }

        /* Removed properties:
         public string author_avatar_big { get; set; }
         public string author_avatar_med { get; set; }
         public string author_avatar_lo { get; set; }
         public object source { get; set; }
         public string type { get; set; }
         public string app { get; set; }
         public string violation_url { get; set; }
         */
    }
}
