using System;

[Serializable]
public class Move_Omok : Move
{
    // 이동의 X, Y 좌표를 나타냅니다.
    public int x;
    public int y;

    // 생성자: x, y 좌표를 받아 객체를 초기화합니다.
    public Move_Omok(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    // Move 클래스의 추상 메서드를 구현합니다.
    // 이 메서드는 수를 놓을 위치가 유효한지 검사하는 데 사용됩니다.
    public override bool IsValid()
    {
        return true;
    }
}