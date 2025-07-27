
using System.Collections.Generic;

namespace avalonia_new.Model
{
    public class ExploreModel
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public List<ExploreItemModel>? FeatureList { get; set; }
    }

    public class ExploreItemModel
    {
        public int Id { get; set; }
        public required string FeatureName { get; set; }
        public required string FontIcon { get; set; }
    }
}
