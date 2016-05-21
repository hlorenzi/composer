using System.Collections.Generic;


namespace Composer.Project
{
    public class Project
    {
        public List<Track> tracks;


        public Project()
        {
            this.tracks = new List<Track>();
        }


        public float TimeInWholeNote
        {
            get { return 960; }
        }
    }
}
