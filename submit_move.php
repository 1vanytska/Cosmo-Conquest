<?php 
require 'db.php';
header('Content-Type: application/json');

$input = file_get_contents('php://input');
$data = json_decode($input, true);

if (json_last_error() !== JSON_ERROR_NONE) {
    http_response_code(400);
    echo json_encode(["error" => "Invalid JSON format"]);
    exit;
}

$required = ['player_id', 'kronus', 'lyrion', 'mystara', 'eclipsia', 'fiora'];
foreach ($required as $field) {
    if (!isset($data[$field])) {
        http_response_code(400);
        echo json_encode(["error" => "Missing field: $field"]);
        exit;
    }
}

$player_id = (int)$data['player_id'];
$kronus = (int)$data['kronus'];
$lyrion = (int)$data['lyrion'];
$mystara = (int)$data['mystara'];
$eclipsia = (int)$data['eclipsia'];
$fiora = (int)$data['fiora'];

try {
    $pdo->beginTransaction();

    $stmt = $pdo->prepare("INSERT INTO games (player_id, kronus, lyrion, mystara, eclipsia, fiora) VALUES (?, ?, ?, ?, ?, ?)");
    $stmt->execute([$player_id, $kronus, $lyrion, $mystara, $eclipsia, $fiora]);

    $updateStmt = $pdo->prepare("UPDATE players SET status = 'submitted' WHERE id = ?");
    $updateStmt->execute([$player_id]);

    $pdo->commit();

    echo json_encode(["status" => "success", "message" => "Move saved and status updated"]);
} catch (PDOException $e) {
    $pdo->rollBack();
    http_response_code(500);
    echo json_encode(["error" => "Database error: " . $e->getMessage()]);
}
