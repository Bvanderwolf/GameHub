public class RelativePositions
{
    public const int LEFTTOP = 1;
    public const int LEFTMIDDLE = 2;
    public const int LEFTBOT = 3;
    public const int RIGHTTOP = 4;
    public const int RIGHTMIDDLE = 5;
    public const int RIGHTBOT = 6;

    public static bool TOP (int _side) => _side == LEFTTOP || _side == RIGHTTOP;

    public static bool BOT (int _side) => _side == LEFTBOT || _side == RIGHTBOT;

    public static bool MIDDLE (int _side) => _side == LEFTMIDDLE || _side == RIGHTMIDDLE;

    public static bool LEFT (int _side) => _side == LEFTMIDDLE || _side == LEFTTOP || _side == LEFTBOT;

    public static bool RIGHT (int _side) => _side == RIGHTMIDDLE || _side == RIGHTTOP || _side == RIGHTBOT;
}