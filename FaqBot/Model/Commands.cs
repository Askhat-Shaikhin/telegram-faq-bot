using System.Collections.Generic;

namespace FaqBot.Model
{
    public class BotCommands
    {
        public List<Command> Commands { get; set; }
    }

    public class Command
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public List<string> Shortcuts { get; set; }
    }
}
