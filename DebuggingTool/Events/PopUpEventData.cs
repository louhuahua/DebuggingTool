using Prism.Events;

namespace DebuggingTool.Events
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
