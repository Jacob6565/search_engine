using SearchEngine.WebCrawler;

namespace SearchEngine.Indexer
{
    public class Indexer
    {
        public PageDB pageDB;
        public void Initialize(PageDB pageDB)
        {
            this.pageDB = pageDB;
            pageDB.LoadPagesFromFiles();
        }

        public void Run()
        {
            string dummyString = "the research in the department has computers, programming, as well as software and computer systems as its field.";


        }

    }
}
