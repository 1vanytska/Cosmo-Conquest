<?php
require 'db.php';
header('Content-Type: application/json');

try {
    $stmt = $pdo->query("SELECT COUNT(*) AS not_submitted FROM players WHERE status != 'submitted'");
    $result = $stmt->fetch(PDO::FETCH_ASSOC);

    if ((int)$result['not_submitted'] === 0) {
        echo json_encode([
            "status" => "ready",
            "message" => "All players have submitted."
        ]);
    } else {
        echo json_encode([
            "status" => "waiting",
            "message" => "Waiting for other players.",
            "remaining" => (int)$result['not_submitted']
        ]);
    }
} catch (PDOException $e) {
    http_response_code(500);
    echo json_encode(["error" => "Database error: " . $e->getMessage()]);
}
