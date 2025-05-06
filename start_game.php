<?php  
require 'db.php';

if ($_SERVER['REQUEST_METHOD'] === 'GET') {
    $stmt = $pdo->query("SELECT COUNT(*) FROM players WHERE status = 'playing'");
    $playing = $stmt->fetchColumn();

    if ($playing > 0) {
        echo json_encode(["status" => "Game started"]);
    } else {
        echo json_encode(["status" => "Waiting"]);
    }
    exit;
}

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $stmt = $pdo->query("SELECT COUNT(*) FROM players WHERE status = 'playing'");
    $alreadyPlaying = $stmt->fetchColumn();

    if ($alreadyPlaying > 0) {
        echo json_encode(["status" => "Game already started"]);
        exit;
    }

    $stmt = $pdo->query("SELECT COUNT(*) FROM players WHERE status = 'waiting'");
    $waiting = $stmt->fetchColumn();

    if ($waiting >= 2) {
        $pdo->query("UPDATE players SET status = 'playing' WHERE status = 'waiting'");
        echo json_encode(["status" => "Game started"]);
    } else {
        echo json_encode(["error" => "Not enough players"]);
    }
}
?>
