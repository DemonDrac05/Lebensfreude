using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ─────────────────────────────────────────
// ENUMS DÙNG CHUNG CHO HỆ THỐNG KINH TẾ
// ─────────────────────────────────────────
// File gom các enum nền tảng để mọi script kinh tế tham chiếu cùng một chỗ,
// tránh khai báo trùng lặp gây conflict khi biên dịch.

// Độ co giãn của cầu theo NHÓM HÀNG (price elasticity of demand - Mankiw).
// Hàng thiết yếu co giãn THẤP (giá ít rớt khi bán nhiều), hàng xa xỉ co giãn CAO.
public enum ElasticityTier
{
    Basic,    // ε = 0.2  — hàng cơ bản (Wood Plank, Stone Brick): giá ổn định
    Metal,    // ε = 0.5  — hàng kim loại (Iron Bar...): nhạy vừa
    Alloy,    // ε = 0.8  — hàng hợp kim (Steel Ingot...): nhạy giá
    Endgame   // ε = 1.2  — hàng cuối game (Legendary...): bán nhiều là sụp giá
}

// 4 giai đoạn hồi sinh của mỗi làng (theo Full Design Document, mục 7).
public enum VillagePhase
{
    Abandoned   = 0,  // chưa tìm thấy / hoang tàn
    Trust       = 1,  // mở shop cơ bản, mua hàng Tier 1-2
    Partnership = 2,  // mở shop hiếm, bán nguyên liệu đặc biệt
    Revival     = 3   // hoàn thành revival quest -> thưởng Artifact
}

// 3 làng cố định trong thế giới.
public enum VillageId
{
    Sylvan,    // Làng thảo dược (Village 1)
    Ironhold,  // Làng khai khoáng (Village 2)
    Aurum      // Làng nghệ nhân (Village 3)
}

// Mảnh Artifact thưởng khi hoàn thành revival quest của từng làng.
public enum ArtifactType
{
    None,
    Forest,    // Sylvan  — phát sáng xanh lá
    Mountain,  // Ironhold — phát sáng cam
    Golden     // Aurum   — phát sáng vàng
}
