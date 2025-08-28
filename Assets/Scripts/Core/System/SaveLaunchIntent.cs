/// <summary>
/// TitleScene에서 ChapterScene으로 로드할 파일명을 전달하는 간단한 정적 홀더 클래스입니다.
/// </summary>
public static class SaveLaunchIntent
{
    public static string PendingFile { get; private set; }
    public static bool HasPending => !string.IsNullOrEmpty(PendingFile);
    public static void SetPendingFile(string fileName)
    {
        PendingFile = fileName;
    }

    public static void Clear() => PendingFile = null;

    public static bool IsNewGame { get; private set; }
    public static RoomType NewGameRoom { get; private set; }
    public static string NewGameTargetKey { get; private set; }

    public static void SetNewGameStartLocation(RoomType room, string key)
    {
        IsNewGame = true;
        NewGameRoom = room;
        NewGameTargetKey = key;
    }

    // 로드 시에는 '새 게임' 상태를 해제
    public static void ClearNewGameState() => IsNewGame = false;
}