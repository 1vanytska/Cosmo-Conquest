<?php
header('Content-Type: application/json');
require_once 'db.php';

try {
    $pdo->exec("DELETE FROM games");
    $pdo->exec("DELETE FROM players");

    echo json_encode(["success" => true]);
} catch (PDOException $e) {
    echo json_encode(["success" => false, "error" => $e->getMessage()]);
}
?>
