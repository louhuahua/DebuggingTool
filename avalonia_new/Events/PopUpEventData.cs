using Prism.Events;

namespace avalonia_new.Events
{
    public class PopUpEventData
    {
        public string? Title { get; set; }
        public bool IsDisplayed { get; set; }

        public PopUpEventData(bool isDisplayed)
        {
            IsDisplayed = isDisplayed;
        }
    }
    public class PopUpEvent : PubSubEvent<PopUpEventData>
    {
    }

}
