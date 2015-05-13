using Mirko_v2.ViewModel;
using System.Collections.Generic;
using System.Linq;

namespace Mirko_v2.Utils
{
    public static class BlacklistHelper
    {
        public enum BlockType
        {
            ENTRY,
            HIDDEN_ENTRIES,
            NULL
        };

        public class Block
        {
            public BlockType Type { get; set; }

            public List<string> Authors { get; set; }
            public List<uint> EntriesIDs { get; set; }
            public string Text { get; set; }

            public CommentViewModel Comment { get; set; }
        }

        public static ObservableCollectionEx<Block> ProcessComments(ObservableCollectionEx<CommentViewModel> comments)
        {
            Block newBlock = null;
            var Blocks = new ObservableCollectionEx<Block>();

            foreach (var comment in comments)
            {
                var data = comment.Data;
                if (data.Blacklisted)
                {
                    // this entry is blocked. instead of showing entire entry, we'll show only a block showing who's the author and how many other entries are blocked in this block.
                    if (Blocks.Any())
                    {
                        var previousBlock = Blocks.Last();
                        if (previousBlock.Type == BlockType.HIDDEN_ENTRIES)
                        {
                            previousBlock.Authors.Add(data.AuthorName);
                            previousBlock.EntriesIDs.Add(data.ID);

                            // add new NULL block
                            Blocks.Add(new Block() 
                            { 
                                Type = BlockType.NULL, 
                                EntriesIDs = new List<uint>() { data.ID },
                                Comment = comment,
                            });
                        }
                        else if (previousBlock.Type == BlockType.NULL)
                        {
                            // find block containing authors list
                            var lastBlock = Blocks.Last(x => x.Type == BlockType.HIDDEN_ENTRIES);
                            lastBlock.Authors.Add(data.AuthorName);
                            previousBlock.EntriesIDs.Add(data.ID);

                            // add new NULL block
                            Blocks.Add(new Block() 
                            { 
                                Type = BlockType.NULL, 
                                EntriesIDs = new List<uint>() { data.ID },
                                Comment = comment,
                            });
                        }
                        else
                        {
                            // add new block containing authors list
                            newBlock = new Block() 
                            { 
                                Type = BlockType.HIDDEN_ENTRIES,
                                Authors = new List<string>() { data.AuthorName },
                                EntriesIDs = new List<uint>() { data.ID },
                                Comment = comment,
                            };
                            Blocks.Add(newBlock);
                        }
                    }
                    else
                    {
                        // no previous blocks in the list.
                        newBlock = new Block()
                        {
                            Type = BlockType.HIDDEN_ENTRIES,
                            Authors = new List<string>() { data.AuthorName },
                            EntriesIDs = new List<uint>() { data.ID },
                            Comment = comment,
                        };
                        Blocks.Add(newBlock);
                    }
                }
                else // comment is not blocked. just a regular entry.
                {
                    newBlock = new Block() 
                    {
                        Type = BlockType.ENTRY,
                        EntriesIDs = new List<uint>() { data.ID },
                        Comment = comment,
                    };
                    Blocks.Add(newBlock);
                }
            }

            // now parse list of blocks. find HIDDEN_ENTRIES and parse their authors list into a nice string.
            var hiddenEntries = Blocks.Where(x => x.Type == BlockType.HIDDEN_ENTRIES);
            foreach (var hiddenEntry in hiddenEntries)
            {
                var authors = hiddenEntry.Authors.Distinct().ToList();
                if (authors.Count() == 1)
                {
                    hiddenEntry.Text = authors[0];
                }
                else if (authors.Count() == 2)
                {
                    hiddenEntry.Text = authors[0] + " i " + authors[1];
                }
                else
                {
                    var authorsLeft = authors.Count - 1;
                    hiddenEntry.Text = authors[0] + " i " + authorsLeft + " innych";
                }
            }

            return Blocks;
        }
    }
}
