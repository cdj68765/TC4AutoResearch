package com.Emil.TCAutoResearch;

import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;

public class SaveToJson {

    public static void saveToFile(String json, String filename) throws Exception {
        Path path = Paths.get(filename);
        Files.write(path, json.getBytes());
    }

    public static String loadFromFile(String filename) throws Exception {
        Path path = Paths.get(filename);
        return new String(Files.readAllBytes(path));
    }
}
