import { useState, useEffect } from "react";
import { MessagingHeader } from "./MessagingHeader";
import { ChatBox } from "./ChatBox";

interface User {
  id: string;
  name: string;
  avatar?: string;
  status: "online" | "offline" | "away" | "busy";
  lastSeen?: string;
  role?: string;
  department?: string;
}

interface ChatBoxData {
  threadId: string;
  isMinimized: boolean;
  isGroup: boolean;
  threadName?: string;
  participants: User[];
}

interface MessagingCoordinatorProps {
  onFullPageOpen: () => void;
  onChatOpen?: (threadId: string) => void;
}

// Mock users data
const mockUsers: User[] = [
  { id: "1", name: "Sarah Wilson", status: "online", role: "Production Manager", department: "Operations" },
  { id: "2", name: "Mike Rodriguez", status: "online", role: "Machine Operator", department: "Production" },
  { id: "3", name: "Jennifer Chen", status: "away", role: "Quality Supervisor", department: "QA" },
  { id: "4", name: "David Thompson", status: "offline", lastSeen: "2 hours ago", role: "Logistics Coordinator", department: "Shipping" },
  { id: "5", name: "Lisa Park", status: "busy", role: "Inventory Manager", department: "Warehouse" },
  { id: "6", name: "James Miller", status: "online", role: "Safety Officer", department: "Safety" },
  { id: "7", name: "Emily Davis", status: "offline", lastSeen: "1 day ago", role: "Maintenance Tech", department: "Maintenance" },
  { id: "8", name: "Robert Johnson", status: "online", role: "Shift Supervisor", department: "Operations" }
];

export function MessagingCoordinator({ onFullPageOpen, onChatOpen }: MessagingCoordinatorProps) {
  const [openChatBoxes, setOpenChatBoxes] = useState<ChatBoxData[]>([]);

  // Handle viewport size for responsive behavior
  const [isMobile, setIsMobile] = useState(false);
  const [isTablet, setIsTablet] = useState(false);

  useEffect(() => {
    const checkViewport = () => {
      const width = window.innerWidth;
      setIsMobile(width < 768);
      setIsTablet(width >= 768 && width < 1024);
    };

    checkViewport();
    window.addEventListener('resize', checkViewport);
    return () => window.removeEventListener('resize', checkViewport);
  }, []);

  const openChatBox = (threadId: string) => {
    // Call external handler if provided
    onChatOpen?.(threadId);
    // Check if chat box is already open
    const existingBox = openChatBoxes.find(box => box.threadId === threadId);
    if (existingBox) {
      // Unminimize if minimized
      if (existingBox.isMinimized) {
        setOpenChatBoxes(prev => prev.map(box => 
          box.threadId === threadId ? { ...box, isMinimized: false } : box
        ));
      }
      return;
    }

    // Determine if it's a group chat and get participants
    let isGroup = false;
    let threadName = "";
    let participants: User[] = [];

    // Mock thread data - in real app this would come from API
    if (threadId === "t1") {
      participants = [mockUsers[0], mockUsers[1]]; // Sarah Wilson & Mike Rodriguez
    } else if (threadId === "t2") {
      isGroup = true;
      threadName = "Safety Review";
      participants = [mockUsers[0], mockUsers[2], mockUsers[5]]; // Sarah, Jennifer, James
    } else if (threadId === "t3") {
      participants = [mockUsers[0], mockUsers[3]]; // Sarah Wilson & David Thompson
    } else if (threadId === "t4") {
      participants = [mockUsers[0], mockUsers[4]]; // Sarah Wilson & Lisa Park
    } else if (threadId === "t5") {
      isGroup = true;
      threadName = "Shift Coordination";
      participants = [mockUsers[0], mockUsers[1], mockUsers[7]]; // Sarah, Mike, Robert
    } else if (threadId.startsWith("t_")) {
      // New direct message thread
      const userId = threadId.split("_")[1];
      const otherUser = mockUsers.find(u => u.id === userId);
      if (otherUser) {
        participants = [mockUsers[0], otherUser]; // Current user + other user
      }
    } else if (threadId.startsWith("tg_")) {
      // New group thread
      isGroup = true;
      threadName = "New Group";
      participants = [mockUsers[0]]; // Start with current user
    }

    // On mobile, open full-screen modal instead of chatbox
    if (isMobile) {
      onFullPageOpen();
      return;
    }

    // Limit to 3 chat boxes on desktop
    const maxChatBoxes = isMobile ? 1 : isTablet ? 2 : 3;
    
    const newChatBox: ChatBoxData = {
      threadId,
      isMinimized: false,
      isGroup,
      threadName,
      participants
    };

    setOpenChatBoxes(prev => {
      const updated = [...prev, newChatBox];
      return updated.slice(-maxChatBoxes); // Keep only the most recent ones
    });
  };

  const closeChatBox = (threadId: string) => {
    setOpenChatBoxes(prev => prev.filter(box => box.threadId !== threadId));
  };

  const minimizeChatBox = (threadId: string) => {
    setOpenChatBoxes(prev => prev.map(box => 
      box.threadId === threadId ? { ...box, isMinimized: !box.isMinimized } : box
    ));
  };

  // Calculate positions for chat boxes
  const getChatBoxPosition = (index: number) => {
    const baseRight = 20;
    const boxWidth = 320;
    const gap = 10;
    
    return {
      bottom: 20,
      right: baseRight + (index * (boxWidth + gap))
    };
  };

  return (
    <>
      {/* Chat Boxes */}
      {!isMobile && openChatBoxes.map((chatBox, index) => (
        <ChatBox
          key={chatBox.threadId}
          threadId={chatBox.threadId}
          onClose={() => closeChatBox(chatBox.threadId)}
          onMinimize={() => minimizeChatBox(chatBox.threadId)}
          isMinimized={chatBox.isMinimized}
          position={getChatBoxPosition(index)}
          isGroup={chatBox.isGroup}
          threadName={chatBox.threadName}
          participants={chatBox.participants}
        />
      ))}
    </>
  );
}