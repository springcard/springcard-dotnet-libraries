using System;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace SpringCard.LibCs
{
    public enum ENotificationType
    {
        Fatal,
        Error,
        Warning,
        Info
    };

    public interface INotificationInteraction
    {
        void Show(string message);
        void Show(ENotificationType type, string message);
        void Show(ENotificationType type, string title, string message);
    }

    public interface IProgressInteraction
    {
        bool ProgressBegin();
        bool Progress(int percent);
        bool ProgressEnd();
    }

    public interface IPromptInteraction
    {
        bool Prompt(string Title, string Prompt, string Default, out string Result);
    }

    public class CPasswordInteractionSettings
    {
        public int MinLength;
        public int MaxLength;
        public string Prompt;
        public string Confirm;
        public string Remark;
        public string Title;
        public bool MayRemember;
        public bool MustRemember;

        public static CPasswordInteractionSettings Create_Password(int MinLength = 8, int MaxLength = 0)
        {
            CPasswordInteractionSettings result = new CPasswordInteractionSettings();
            result.MinLength = MinLength;
            result.MaxLength = MaxLength;
            return result;
        }

        public static CPasswordInteractionSettings Create_Passphrase(int MinLength = 24, int MaxLength = 0)
        {
            CPasswordInteractionSettings result = new CPasswordInteractionSettings();
            result.MinLength = MinLength;
            result.MaxLength = MaxLength;
            return result;
        }
    }

    public interface IPasswordInteraction
    {
        bool EnterPassword(CPasswordInteractionSettings Settings, out string Password);
        bool CreatePassword(CPasswordInteractionSettings Settings, out string Password);
    }


    public class QuietInteraction : INotificationInteraction
    {
        ENotificationType type;
        string title;
        string message;

        public ENotificationType Type
        {
            get
            {
                return type;
            }
        }

        public string Title
        {
            get
            {
                return title;
            }
        }

        public string Message
        {
            get
            {
                return message;
            }
        }

        public void Show(string message)
        {
            this.type = ENotificationType.Info;
            this.title = null;
            this.message = message;
            Logger.Trace(message);
        }

        public void Show(ENotificationType type, string message)
        {
            this.type = type;
            this.title = null;
            this.message = message;
            Logger.Trace("[{0}] {1}", type.ToString(), message);
        }

        public void Show(ENotificationType type, string title, string message)
        {
            this.type = type;
            this.title = title;
            this.message = message;
            Logger.Trace("[{0}] {1}\t{2}", type.ToString(), title, message);
        }
    }

    public class ConsoleInteraction : INotificationInteraction, IPromptInteraction, IPasswordInteraction, IProgressInteraction
    {
        public void Show(string message)
        {
            Console.WriteLine(message);
        }

        public void Show(ENotificationType type, string message)
        {
            Console.WriteLine("[{0}] {1}", type.ToString(), message);
        }

        public void Show(ENotificationType type, string title, string message)
        {
            Console.WriteLine("[{0}] {1}\t{2}", type.ToString(), title, message);
        }

        public bool Prompt(string Title, string Prompt, string Default, out string Result)
        {
            if (!string.IsNullOrEmpty(Title))
                Console.WriteLine(Title);

            if (string.IsNullOrEmpty(Prompt))
                Prompt = "?";

            if (Default != null)
            {
                Console.Write("{0} [{1}]: ", Prompt, Default);
            }
            else
            {
                Console.Write("{0}: ", Prompt);
            }

            string inputText = Console.ReadLine().Trim();

            if (string.IsNullOrEmpty(inputText))
                Result = Default;
            else
                Result = inputText;

            return true;
        }

        public bool EnterPassword(CPasswordInteractionSettings Settings, out string Password)
        {
            if (Settings == null)
                Settings = new CPasswordInteractionSettings();

            Logger.Debug("Enter password");

            if (!string.IsNullOrEmpty(Settings.Title))
                Console.WriteLine(Settings.Title);
            if (!string.IsNullOrEmpty(Settings.Remark))
                Console.WriteLine("\t{0}", Settings.Remark);

            if (!string.IsNullOrEmpty(Settings.Prompt))
                Console.Write("{0}: ", Settings.Prompt);
            else
                Console.Write("Enter password: ");

            if (!doReadPassword(out Password))
                return false;

            return true;
        }

        public bool CreatePassword(CPasswordInteractionSettings Settings, out string Password)
        {
            if (Settings == null)
                Settings = new CPasswordInteractionSettings();

            if (!string.IsNullOrEmpty(Settings.Title))
                Console.WriteLine(Settings.Title);
            if (!string.IsNullOrEmpty(Settings.Remark))
                Console.WriteLine("\t{0}", Settings.Remark);

            Logger.Debug("Create password, min {0}, max {1}", Settings.MinLength, Settings.MaxLength);

            while (true)
            {
                Password = null;

                if (!string.IsNullOrEmpty(Settings.Prompt))
                    Console.Write("{0}: ", Settings.Prompt);
                else
                    Console.Write("Enter new password: ");

                if (!doReadPassword(out string password1))
                    return false;

                if ((Settings.MinLength > 0) && (password1.Length < Settings.MinLength))
                {
                    Console.WriteLine("This password is too short ({0} characters min)", Settings.MinLength);
                    continue;
                }
                if ((Settings.MaxLength > 0) && (password1.Length > Settings.MaxLength))
                {
                    Console.WriteLine("This password is too long ({0} characters max)", Settings.MaxLength);
                    continue;
                }

                if (!string.IsNullOrEmpty(Settings.Confirm))
                    Console.Write("{0}: ", Settings.Confirm);
                else
                    Console.Write("Confirm new password: ");

                if (!doReadPassword(out string password2))
                    return false;

                if (password1 != password2)
                {
                    Console.WriteLine("Passwords don't match");
                    continue;
                }

                Password = password1;
                return true;
            }
        }

        private bool doReadPassword(out string password)
        {
            password = "";
            do
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine(" (Escape)");
                    return false; /* Escape */
                }

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                    {
                        password = password.Substring(0, (password.Length - 1));
                        Console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            } while (true);

            Console.WriteLine();
            return true;
        }

        const char _block = '■';
        const string _back = "\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b";
        const string _twirl = "-\\|/";

        public bool Progress(bool first, int percent)
        {
            if (!first)
                Console.Write(_back);

            Console.Write("[");
            var p = (int)((percent / 50f) + .5f);
            for (var i = 0; i < 50; ++i)
            {
                if (i >= p)
                    Console.Write(' ');
                else
                    Console.Write(_block);
            }
            Console.Write("] {0,3:##0}%", percent);

            return true;
        }

        public bool ProgressBegin()
        {
            return Progress(true, 0);
        }

        public bool Progress(int percent)
        {
            return Progress(false, percent);
        }

        public bool ProgressEnd()
        {
            bool result = Progress(false, 100);
            Console.WriteLine();
            return result;
        }
    }

    public class QuietPasswordInteraction : IPasswordInteraction
    {
        string Password;

        public QuietPasswordInteraction(string Password)
        {
            this.Password = Password;
        }

        public bool CreatePassword(CPasswordInteractionSettings Settings, out string Password)
        {
            Password = this.Password;
            return true;
        }

        public bool EnterPassword(CPasswordInteractionSettings Settings, out string Password)
        {
            Password = this.Password;
            return true;
        }
    }


#if NON
    public static bool ToConsole;

        InteractionType type;
        string message;
        object[] args;

        public Interaction(InteractionType type, string message, params object[] args)
        {
            this.type = type;
            this.message = message;
            this.args = args;

            if (ToConsole)
                WriteToConsole();
        }

        public Interaction(string message, params object[] args)
        {
            this.type = InteractionType.Info;
            this.message = message;
            this.args = args;

            if (ToConsole)
                WriteToConsole();
        }

        public Interaction(InteractionType type, string message)
        {
            this.type = type;
            this.message = message;
            this.args = null;

            if (ToConsole)
                WriteToConsole();
        }

        public Interaction(string message)
        {
            this.type = InteractionType.Info;
            this.message = message;
            this.args = null;

            if (ToConsole)
                WriteToConsole();
        }

        public string Message
        {
            get
            {
                if (args != null)
                    return string.Format(message, args);
                return message;
            }
        }

        public void WriteToConsole()
        {
            Console.WriteLine("[{0}] {1}", type.ToString(), Message);
        }

        public static string GetMessage(Interaction interaction)
        {
            if (interaction == null)
                return "Internal error";

            return interaction.Message;
        }
    }
#endif
}
