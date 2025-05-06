<?php 
require 'db.php';

if ($_SERVER['REQUEST_METHOD'] === 'POST' && basename($_SERVER['PHP_SELF']) === 'register.php') {
    $data = json_decode(file_get_contents("php://input"), true);
    $username = trim($data['username'] ?? '');

    if (empty($username)) {
        echo json_encode(["error" => "Username is required"]);
        exit;
    }

    $stmt = $pdo->prepare("SELECT id FROM players WHERE username = :username");
    $stmt->execute(['username' => $username]);
    if ($stmt->fetchColumn()) {
        echo json_encode(["error" => "Username is already taken"]);
        exit;
    }

    $stmt = $pdo->prepare("INSERT INTO players (username, status) VALUES (:username, 'waiting') RETURNING id");
    $stmt->execute(['username' => $username]);
    $player_id = $stmt->fetchColumn();

    file_put_contents('register.log', date("Y-m-d H:i:s") . " - Registered: $username (ID: $player_id)\n", FILE_APPEND);

    echo json_encode(["player_id" => $player_id]);
}
?>
