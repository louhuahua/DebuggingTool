using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebuggingTool.Model;

public class SnackBarMessage
{
    public string Message { get; set; }
    public int Duration { get; set; }
    public string ActionText { get; set; }
    public Action Action { get; set; }

    public SnackBarMessage(
        string message,
        int duration,
        string actionText = null,
        Action action = null
    )
    {
        Message = message;
        Duration = duration;
        ActionText = actionText;
        Action = action;
    }
}
