package com.Emil.TCAutoResearch;

import static com.Emil.TCAutoResearch.ResearchCurrectNote.*;
import static com.Emil.TCAutoResearch.SolvesNote.*;

import java.io.BufferedReader;
import java.io.File;
import java.io.InputStreamReader;
import java.util.*;

import net.minecraft.client.Minecraft;
import net.minecraft.entity.player.EntityPlayer;

import thaumcraft.api.aspects.Aspect;
import thaumcraft.api.aspects.AspectList;
import thaumcraft.client.gui.GuiResearchTable;
import thaumcraft.common.Thaumcraft;
import thaumcraft.common.lib.research.ResearchManager;

public class AutoResearch extends Thread {

    public static boolean Stop = false;

    public AutoResearch(EntityPlayer Player, Minecraft mc, GuiResearchTableHelperInterface obj) {
        player = Player;
        ResearchCurrectNote.mc = mc;
        guiResearchTable = (GuiResearchTable) obj;
        ResearchCurrectNote.GuiResearchTableHelperInterfaceObj = obj;
        Stop = false;
    }

    public AutoResearch() {

    }

    @Override
    public synchronized void start() {
        super.start();
    }

    int LastNote = 0;

    @Override
    public void run() {
        while (!Stop) {
            try {
                sleep(1000);
                if (guiResearchTable.note != null &&
                    !guiResearchTable.note.complete &&
                    SolvesNote.LastNote != null &&
                    !SolvesNote.LastNote.isEmpty() &&
                    LastNoteID != 0 )
                {
                    var NewNote = guiResearchTable.note;
                    if (NewNote.hashCode()==LastNoteID) {
                        SolvesNote.SolvesNoteHandle(SolvesNote.LastNote);
                    }
                }
                if (guiResearchTable.note != null && !guiResearchTable.note.complete
                    && LastNote!=(guiResearchTable.note.hashCode()))
                {
                    System.out.println(guiResearchTable.note.hashCode());

                    HashMap<String, ResearchManager.HexEntry> targetItems = new HashMap<>();
                    LastNote = guiResearchTable.note.hashCode();
                    var NewNote = guiResearchTable.note;
                    SolvesNote.LastNote = "";
                    SolvesNote.LastNoteID = guiResearchTable.note.hashCode();
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
                            WaitSend += aspect.getTag() + ":"
                                + Components[0].getTag()
                                + ":"
                                + Components[1].getTag()
                                + "&";
                        } else {
                            WaitSend += aspect.getTag() + "&";
                        }
                    }
                    WaitSend += "^";
                    for (Map.Entry<String, ResearchManager.HexEntry> entry : guiResearchTable.note.hexEntries
                        .entrySet()) {
                        var Path = entry.getKey();
                        var Entry = entry.getValue();
                        if (Entry.aspect != null) WaitSend += Path + ":" + Entry.aspect.getTag() + "&";
                        else WaitSend += Path + "&";
                    }
                    ProcessBuilder builder = new ProcessBuilder(new File("AutoResearch.dll").toString(), WaitSend);
                    //ProcessBuilder builder = new ProcessBuilder(new File("AutoResearch\\bin\\Debug\\net9.0\\AutoResearch.exe").toString(),WaitSend);
                    //ProcessBuilder builder = new ProcessBuilder(new File("C:\\Users\\GongSi\\Desktop\\TC4Helper-master\\AutoResearch\\bin\\Debug\\net9.0\\AutoResearch.exe").toString(),WaitSend);
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
                    }
                }
            } catch (Exception e) {

            }finally {
                PID=-1;
            }
        }
    }
}
