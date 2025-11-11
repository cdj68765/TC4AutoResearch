package com.Emil.TCAutoResearch;

import java.io.*;
import java.util.zip.ZipEntry;
import java.util.zip.ZipInputStream;

import net.minecraftforge.common.config.Configuration;

public class Config {

    public static boolean AutoResearch;
    public static Configuration config;
    public static File ConbfigFilePath;

    public static void synchronizeConfiguration(File configFile) {
        ConbfigFilePath = new File(configFile.getParent(), "TCAutoResearch.cfg");
        config = new Configuration(ConbfigFilePath);
        AutoResearch = config
            .getBoolean("AutoResearch", Configuration.CATEGORY_GENERAL, false, "Enable the Research Auto Start");
        var AutoResearch = new File("AutoResearch.dll");
        if (!AutoResearch.exists()||AutoResearch.length()!=2222592) extractNativeFromZip();
        if (config.hasChanged()) {
            config.save();
        }
    }

    public static void SaveConfiguration() {
        config.get(Configuration.CATEGORY_GENERAL, "AutoResearch", false)
            .set(AutoResearch);
        config.save();
    }

    public static void extractNativeFromZip() {

        try (InputStream zipStream = AutoResearch.class.getResourceAsStream("/AutoResearch.zip");
            ZipInputStream zis = new ZipInputStream(zipStream)) {

            ZipEntry entry;
            while ((entry = zis.getNextEntry()) != null) {
                try (FileOutputStream fos = new FileOutputStream(new File(entry.getName()).getName())) {
                    byte[] buffer = new byte[4096];
                    int len;
                    while ((len = zis.read(buffer)) > 0) {
                        fos.write(buffer, 0, len);
                    }
                }
            }
        } catch (IOException e) {
            e.printStackTrace();
        }

    }
}
