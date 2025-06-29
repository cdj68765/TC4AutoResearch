package com.Emil.TCAutoResearch;

import static java.lang.Thread.sleep;

import java.io.*;
import java.util.*;
import java.util.concurrent.atomic.AtomicBoolean;

import net.minecraft.client.Minecraft;
import net.minecraft.entity.player.EntityPlayer;

import thaumcraft.api.aspects.Aspect;
import thaumcraft.api.aspects.AspectList;
import thaumcraft.client.gui.GuiResearchTable;
import thaumcraft.client.lib.PlayerNotifications;
import thaumcraft.common.Thaumcraft;
import thaumcraft.common.lib.research.ResearchManager;

public class ResearchCurrectNote {

    public static EntityPlayer player;
    public static GuiResearchTable guiResearchTable;
    public static GuiResearchTableHelperInterface GuiResearchTableHelperInterfaceObj;
    public static Minecraft mc;
    static long lastClickTime = 0;

    public static void ResearchNote(GuiResearchTableHelperInterface GuiResearchTable, EntityPlayer Player,
        Minecraft MC) {
        player = Player;
        mc = MC;
        guiResearchTable = (GuiResearchTable) GuiResearchTable;
        GuiResearchTableHelperInterfaceObj = GuiResearchTable;
        long now = System.currentTimeMillis();
        if (now - lastClickTime < 2000) return;
        lastClickTime = now;
        if (guiResearchTable.note == null) {
            PlayerNotifications.addNotification("没有找到研究笔记");
            return;
        } else if (guiResearchTable.note.complete) {
            PlayerNotifications.addNotification("研究笔记已经完成了");
            return;
        }
        new Thread(new Runnable() {
            @Override
            public void run() {
                if(PID!=-1)
                {
                    AtomicBoolean CheckConnect= new AtomicBoolean(true);
                    AtomicBoolean ConnectStart= new AtomicBoolean(false);
                    MC.displayGuiScreen(
                        new GuiMessageBox(MC.currentScreen, "检测到上次解锁并未结束,是否强制停止上次结果(点击否就直接退出不做任何操作,点击是就强制关闭上次,进行本次解锁)", PID, () ->
                        {
                            try {
                                Runtime.getRuntime().exec("taskkill /F /PID " + PID);
                            } catch (Exception e)
                            {
                            }
                            ConnectStart.set(true);
                        }, () ->
                        {
                            CheckConnect.set(false);
                            ConnectStart.set(true);
                        }));
                    while (!ConnectStart.get()) {
                        try {
                            sleep(500);
                        } catch (InterruptedException e) {
                        }
                    }
                    if(!CheckConnect.get())return;
                    PID=-1;
                }
                HashMap<String, ResearchManager.HexEntry> targetItems = new HashMap<>();
                var NewNote = guiResearchTable.note;
                SolvesNote.LastNote = "";
                SolvesNote.LastNoteID = guiResearchTable.note.key;
                NewNote.hexEntries.forEach((key, value) -> {
                    if (value.aspect != null) {
                        targetItems.put(key, value);
                    }
                });
                List<Map<Aspect, Aspect>> CombineLink = new ArrayList<>();
                var PlayaspectList = Thaumcraft.proxy.getPlayerKnowledge()
                    .getAspectsDiscovered(player.getCommandSenderName());
                AspectList finalPlayaspectList = PlayaspectList;
                targetItems.forEach((key, value) -> {
                    var GetAmount = finalPlayaspectList.getAmount(value.aspect);
                    if (GetAmount == 0) {
                        FindCombine(finalPlayaspectList, value.aspect, CombineLink);
                    }
                });
                if (!CombineLink.isEmpty()) {
                    Collections.reverse(CombineLink);
                    CombineLink.forEach((item) -> {
                        var First = item.entrySet()
                            .iterator()
                            .next();
                        GuiResearchTableHelperInterfaceObj.combine(First.getKey(), First.getValue());
                        try {
                            sleep(100);
                        } catch (InterruptedException e) {}
                    });
                }
                PlayaspectList = Thaumcraft.proxy.getPlayerKnowledge()
                    .getAspectsDiscovered(player.getCommandSenderName());
                String WaitSend = "";
                for (Map.Entry<Aspect, Integer> entry : PlayaspectList.aspects.entrySet()) {
                    Aspect aspect = entry.getKey();
                    int amount = entry.getValue();
                    WaitSend += aspect.getTag() + ":" + amount + "&";
                }
                WaitSend += "^";
                for (Map.Entry<String, Aspect> entry : Aspect.aspects.entrySet()) {
                    Aspect aspect = entry.getValue();
                    var Components = aspect.getComponents();
                    if (Components != null) {
                        WaitSend += aspect.getTag() + ":" + Components[0].getTag() + ":" + Components[1].getTag() + "&";
                    } else {
                        WaitSend += aspect.getTag() + "&";
                    }
                }
                WaitSend += "^";
                for (Map.Entry<String, ResearchManager.HexEntry> entry : guiResearchTable.note.hexEntries.entrySet()) {
                    var Path = entry.getKey();
                    var Entry = entry.getValue();
                    if (Entry.aspect != null) WaitSend += Path + ":" + Entry.aspect.getTag() + "&";
                    else WaitSend += Path + "&";
                }
                //ProcessBuilder builder = new ProcessBuilder(new File("AutoResearch\\bin\\Debug\\net9.0\\AutoResearch.exe").toString(),WaitSend);
                ProcessBuilder builder = new ProcessBuilder(new File("AutoResearch.dll").toString(), WaitSend);
                try {
                    Process process = builder.start();
                    Process proc = Runtime.getRuntime().exec("tasklist");
                    BufferedReader reader2 = new BufferedReader(new InputStreamReader(proc.getInputStream()));
                    String line2;
                    while ((line2 = reader2.readLine()) != null) {
                        if (line2.contains("AutoResearch")) {
                            String[] parts = line2.trim().split("\\s+");
                            if (parts.length >= 2) {
                                 PID = Integer.parseInt(parts[1]);
                                 break;
                            }
                        }
                    }
                    BufferedReader reader = new BufferedReader(new InputStreamReader(process.getInputStream()));
                    String line;
                    while ((line = reader.readLine()) != null) {
                        SolvesNote.SolvesNoteHandle(line);
                    }
                    int exitCode = process.waitFor();
                } catch (Exception e) {
                    System.out.println(e.getMessage());
                    System.out.println(e.getStackTrace());
                }finally {
                    PID=-1;
                }
            }
        }).start();
    }
    public static long PID=-1;
    static void FindCombine(AspectList playaspectList, Aspect aspect, List<Map<Aspect, Aspect>> combineLink) {
        var Comps = aspect.getComponents();
        if (Comps != null) {
            Map<Aspect, Aspect> map1 = new HashMap<>();
            map1.put(Comps[0], Comps[1]);
            combineLink.add(map1);
            if (playaspectList.getAmount(Comps[0]) == 0) {
                FindCombine(playaspectList, Comps[0], combineLink);
            }
            if (playaspectList.getAmount(Comps[1]) == 0) {
                FindCombine(playaspectList, Comps[1], combineLink);
            }
        }
    }
}
