using System.Collections.Generic;

/// <summary>
/// AI의 계산 결과를 저장하고 재사용하기 위한 트랜스포지션 테이블입니다.
/// Zobrist Hashing을 사용하여 각 보드 상태의 고유 해시 값을 계산합니다.
/// </summary>
public class TranspositionTable
{
    // 보드 상태의 평가 결과를 저장할 항목
    public struct Entry
    {
        public float score; // 평가 점수
        public int depth;   // 이 점수가 계산된 탐색 깊이
    }

    // 해시 테이블 본체 (Key: 보드 상태 해시, Value: 평가 결과)
    private Dictionary<long, Entry> table = new Dictionary<long, Entry>();

    // Zobrist Hashing을 위한 랜덤 숫자 배열
    // zobristKeys[돌 종류, Y좌표, X좌표]
    private long[,,] zobristKeys;
    private const int BOARD_SIZE = 15; // BoardOmok과 반드시 일치해야 합니다.

    public TranspositionTable()
    {
        // 조브리스트 키 초기화 (매우 큰 랜덤 숫자로 채움)
        zobristKeys = new long[3, BOARD_SIZE, BOARD_SIZE];
        var random = new System.Random();
        for (int piece = 1; piece <= 2; piece++)
        {
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                for (int x = 0; x < BOARD_SIZE; x++)
                {
                    byte[] buffer = new byte[8];
                    random.NextBytes(buffer);
                    zobristKeys[piece, y, x] = System.BitConverter.ToInt64(buffer, 0);
                }
            }
        }
    }

    /// <summary>
    /// 현재 보드 상태의 Zobrist Hash 값을 계산합니다.
    /// </summary>
    public long ComputeHash(int[,] board)
    {
        long hash = 0;
        for (int y = 0; y < BOARD_SIZE; y++)
        {
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                int piece = board[y, x];
                if (piece != 0)
                {
                    // 각 돌의 위치에 해당하는 랜덤 키를 XOR 연산하여 해시를 누적
                    hash ^= zobristKeys[piece, y, x];
                }
            }
        }
        return hash;
    }

    /// <summary>
    /// 테이블에서 현재 보드 상태의 저장된 평가 결과를 가져옵니다.
    /// </summary>
    /// <returns>찾았으면 true, 못 찾았으면 false</returns>
    public bool Probe(long hash, int depth, out float score)
    {
        score = 0;
        if (table.TryGetValue(hash, out Entry entry))
        {
            // 저장된 결과의 탐색 깊이가 현재 탐색 깊이보다 크거나 같아야 유효
            if (entry.depth >= depth)
            {
                score = entry.score;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 새로운 평가 결과를 테이블에 저장합니다.
    /// </summary>
    public void Store(long hash, int depth, float score)
    {
        table[hash] = new Entry { depth = depth, score = score };
    }
}