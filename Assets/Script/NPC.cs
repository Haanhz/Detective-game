using UnityEngine;

// Đảm bảo script này xuất hiện dưới dạng Component trong Inspector
public class NPC : MonoBehaviour
{
    // THÊM: Tên và Ảnh của NPC (Dữ liệu riêng biệt)
    [Header("NPC Identity")]
    public string npcName = "NPC Name"; 
    public Sprite portrait; // Ảnh đại diện của NPC
    
    [Header("Lines for this specific NPC")]
    [TextArea(3, 10)] // Giúp nhập liệu dễ hơn trong Inspector
    public string[] lines;
}