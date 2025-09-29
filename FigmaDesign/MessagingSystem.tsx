import { useState, useEffect } from "react";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { Input } from "./ui/input";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "./ui/tabs";
import { ScrollArea } from "./ui/scroll-area";
import { Avatar, AvatarFallback, AvatarImage } from "./ui/avatar";
import { Card, CardContent, CardHeader, CardTitle } from "./ui/card";
import {
  MessageCircle,
  X,
  Search,
  Users,
  MessageSquare,
  Phone,
  Video,
  MoreHorizontal,
  Send,
  Smile,
  Paperclip,
  ThumbsUp,
  Eye,
  EyeOff
} from "lucide-react";
import { toast } from "sonner@2.0.3";

// Types
interface User {
  id: string;
  name: string;
  avatar?: string;
  status: "online" | "offline" | "away" | "busy";
  lastSeen?: string;
  role?: string;
  department?: string;
}

interface Message {
  id: string;
  senderId: string;
  content: string;
  timestamp: string;
  edited?: boolean;
  reactions?: { emoji: string; users: string[] }[];
  seen: boolean;
  mentions?: string[];
}

interface ChatThread {
  id: string;
  participants: string[];
  name?: string;
  isGroup: boolean;
  lastMessage?: Message;
  unreadCount: number;
  pinned: boolean;
  muted: boolean;
  avatar?: string;
}

interface Group {
  id: string;
  name: string;
  description?: string;
  members: string[];
  avatar?: string;
  isPublic: boolean;
  adminIds: string[];
  createdAt: string;
}

// Mock data
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

const mockGroups: Group[] = [
  {
    id: "g1",
    name: "Production Team",
    description: "Daily production coordination and updates",
    members: ["1", "2", "3", "8"],
    isPublic: false,
    adminIds: ["1", "8"],
    createdAt: "2024-01-15T08:00:00Z"
  },
  {
    id: "g2",
    name: "Safety Committee",
    description: "Safety incidents, training, and compliance discussions",
    members: ["1", "6", "7", "8"],
    isPublic: true,
    adminIds: ["6"],
    createdAt: "2024-01-10T10:00:00Z"
  },
  {
    id: "g3",
    name: "Shift Handoff",
    description: "Communication between shifts",
    members: ["1", "2", "4", "5", "8"],
    isPublic: false,
    adminIds: ["1"],
    createdAt: "2024-01-20T06:00:00Z"
  }
];

const mockThreads: ChatThread[] = [
  {
    id: "t1",
    participants: ["1", "2"],
    isGroup: false,
    lastMessage: {
      id: "m1",
      senderId: "2",
      content: "The slitter line is ready for the next job. Quality check passed.",
      timestamp: "2024-01-31T14:30:00Z",
      seen: false,
      reactions: [{ emoji: "👍", users: ["1"] }]
    },
    unreadCount: 1,
    pinned: false,
    muted: false
  },
  {
    id: "t2",
    participants: ["1", "3", "6"],
    name: "Safety Review",
    isGroup: true,
    lastMessage: {
      id: "m2",
      senderId: "6",
      content: "Please review the new safety protocols before tomorrow's meeting.",
      timestamp: "2024-01-31T13:45:00Z",
      seen: true
    },
    unreadCount: 0,
    pinned: true,
    muted: false
  },
  {
    id: "t3",
    participants: ["1", "4"],
    isGroup: false,
    lastMessage: {
      id: "m3",
      senderId: "4",
      content: "Load T-401 is ready for dispatch. Customer confirmed delivery window.",
      timestamp: "2024-01-31T12:15:00Z",
      seen: true
    },
    unreadCount: 0,
    pinned: false,
    muted: false
  }
];

interface MessageDropdownProps {
  isOpen: boolean;
  onClose: () => void;
  onChatOpen: (threadId: string, threadName?: string, isGroup?: boolean, participants?: User[]) => void;
  onFullPageOpen: () => void;
}

export function MessageDropdown({ isOpen, onClose, onChatOpen, onFullPageOpen }: MessageDropdownProps) {
  const [activeTab, setActiveTab] = useState("threads");
  const [searchQuery, setSearchQuery] = useState("");

  if (!isOpen) return null;

  const filteredThreads = mockThreads.filter(thread => {
    if (!searchQuery) return true;
    
    const participantNames = thread.participants
      .map(id => mockUsers.find(u => u.id === id)?.name || "")
      .join(" ");
    
    return participantNames.toLowerCase().includes(searchQuery.toLowerCase()) ||
           thread.name?.toLowerCase().includes(searchQuery.toLowerCase());
  });

  const filteredContacts = mockUsers.filter(user =>
    user.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    user.role?.toLowerCase().includes(searchQuery.toLowerCase()) ||
    user.department?.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const filteredGroups = mockGroups.filter(group =>
    group.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    group.description?.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const getStatusColor = (status: string) => {
    switch (status) {
      case "online": return "bg-green-500";
      case "away": return "bg-yellow-500";
      case "busy": return "bg-red-500";
      default: return "bg-gray-400";
    }
  };

  const formatTime = (timestamp: string) => {
    const date = new Date(timestamp);
    const now = new Date();
    const diffHours = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60));
    
    if (diffHours < 1) return "Just now";
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffHours < 168) return `${Math.floor(diffHours / 24)}d ago`;
    return date.toLocaleDateString();
  };

  return (
    <div className="absolute right-0 top-12 w-80 bg-white border rounded-lg shadow-lg z-50">
      <div className="p-4 border-b">
        <div className="flex items-center justify-between mb-3">
          <h3 className="font-medium">Messages</h3>
          <div className="flex items-center gap-2">
            <Button variant="ghost" size="sm" onClick={onFullPageOpen}>
              <MessageSquare className="h-4 w-4" />
            </Button>
            <Button variant="ghost" size="sm" onClick={onClose}>
              <X className="h-4 w-4" />
            </Button>
          </div>
        </div>
        
        <div className="relative">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search messages..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="pl-9"
          />
        </div>
      </div>

      <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
        <TabsList className="grid w-full grid-cols-3 px-4">
          <TabsTrigger value="threads" className="text-xs">
            Chat Threads
            {mockThreads.reduce((sum, t) => sum + t.unreadCount, 0) > 0 && (
              <Badge className="ml-1 h-4 w-4 p-0 text-xs bg-red-500">
                {mockThreads.reduce((sum, t) => sum + t.unreadCount, 0)}
              </Badge>
            )}
          </TabsTrigger>
          <TabsTrigger value="contacts" className="text-xs">Contacts</TabsTrigger>
          <TabsTrigger value="groups" className="text-xs">Groups</TabsTrigger>
        </TabsList>

        <div className="max-h-96 overflow-y-auto">
          <TabsContent value="threads" className="mt-0 p-0">
            {filteredThreads.length === 0 ? (
              <div className="p-4 text-center text-muted-foreground">
                No chat threads found
              </div>
            ) : (
              <div className="space-y-1 p-2">
                {filteredThreads.map((thread) => {
                  const otherParticipant = thread.isGroup 
                    ? null 
                    : mockUsers.find(u => u.id === thread.participants.find(id => id !== "1"));
                  
                  return (
                    <button
                      key={thread.id}
                      onClick={() => {
                        onChatOpen(thread.id);
                        onClose();
                      }}
                      className="w-full p-3 rounded-lg hover:bg-gray-50 text-left transition-colors"
                    >
                      <div className="flex items-start gap-3">
                        <div className="relative">
                          <Avatar className="h-10 w-10">
                            <AvatarFallback>
                              {thread.isGroup 
                                ? thread.name?.slice(0, 2).toUpperCase()
                                : otherParticipant?.name.slice(0, 2).toUpperCase()
                              }
                            </AvatarFallback>
                          </Avatar>
                          {!thread.isGroup && otherParticipant && (
                            <div className={`absolute -bottom-1 -right-1 h-3 w-3 rounded-full border-2 border-white ${getStatusColor(otherParticipant.status)}`} />
                          )}
                        </div>
                        
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center justify-between">
                            <p className="font-medium text-sm truncate">
                              {thread.isGroup ? thread.name : otherParticipant?.name}
                            </p>
                            <div className="flex items-center gap-1">
                              {thread.pinned && <span className="text-xs">📌</span>}
                              {thread.unreadCount > 0 && (
                                <Badge className="h-5 w-5 p-0 text-xs bg-red-500">
                                  {thread.unreadCount}
                                </Badge>
                              )}
                            </div>
                          </div>
                          
                          {thread.lastMessage && (
                            <div className="flex items-center justify-between mt-1">
                              <p className="text-xs text-muted-foreground truncate">
                                {thread.lastMessage.content}
                              </p>
                              <span className="text-xs text-muted-foreground ml-2">
                                {formatTime(thread.lastMessage.timestamp)}
                              </span>
                            </div>
                          )}
                        </div>
                      </div>
                    </button>
                  );
                })}
              </div>
            )}
          </TabsContent>

          <TabsContent value="contacts" className="mt-0 p-0">
            {filteredContacts.length === 0 ? (
              <div className="p-4 text-center text-muted-foreground">
                No contacts found
              </div>
            ) : (
              <div className="space-y-1 p-2">
                {filteredContacts.map((user) => (
                  <button
                    key={user.id}
                    onClick={() => {
                      // Create or find existing thread with this user
                      const existingThread = mockThreads.find(t => 
                        !t.isGroup && t.participants.includes(user.id)
                      );
                      if (existingThread) {
                        onChatOpen(existingThread.id);
                      } else {
                        // Create new thread
                        const newThreadId = `t_${user.id}_${Date.now()}`;
                        onChatOpen(newThreadId);
                      }
                      onClose();
                    }}
                    className="w-full p-3 rounded-lg hover:bg-gray-50 text-left transition-colors"
                  >
                    <div className="flex items-center gap-3">
                      <div className="relative">
                        <Avatar className="h-10 w-10">
                          <AvatarFallback>{user.name.slice(0, 2).toUpperCase()}</AvatarFallback>
                        </Avatar>
                        <div className={`absolute -bottom-1 -right-1 h-3 w-3 rounded-full border-2 border-white ${getStatusColor(user.status)}`} />
                      </div>
                      
                      <div className="flex-1 min-w-0">
                        <p className="font-medium text-sm">{user.name}</p>
                        <p className="text-xs text-muted-foreground">
                          {user.role} • {user.department}
                        </p>
                        {user.status === "offline" && user.lastSeen && (
                          <p className="text-xs text-muted-foreground">
                            Last seen {user.lastSeen}
                          </p>
                        )}
                      </div>
                    </div>
                  </button>
                ))}
              </div>
            )}
          </TabsContent>

          <TabsContent value="groups" className="mt-0 p-0">
            {filteredGroups.length === 0 ? (
              <div className="p-4 text-center text-muted-foreground">
                No groups found
              </div>
            ) : (
              <div className="space-y-1 p-2">
                {filteredGroups.map((group) => (
                  <button
                    key={group.id}
                    onClick={() => {
                      // Find or create thread for this group
                      const existingThread = mockThreads.find(t => 
                        t.isGroup && t.name === group.name
                      );
                      if (existingThread) {
                        onChatOpen(existingThread.id);
                      } else {
                        // Create new thread for group
                        const newThreadId = `tg_${group.id}_${Date.now()}`;
                        onChatOpen(newThreadId);
                      }
                      onClose();
                    }}
                    className="w-full p-3 rounded-lg hover:bg-gray-50 text-left transition-colors"
                  >
                    <div className="flex items-center gap-3">
                      <Avatar className="h-10 w-10">
                        <AvatarFallback>
                          <Users className="h-5 w-5" />
                        </AvatarFallback>
                      </Avatar>
                      
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2">
                          <p className="font-medium text-sm">{group.name}</p>
                          {group.isPublic && (
                            <Badge variant="outline" className="text-xs">Public</Badge>
                          )}
                        </div>
                        <p className="text-xs text-muted-foreground truncate">
                          {group.description}
                        </p>
                        <p className="text-xs text-muted-foreground">
                          {group.members.length} members
                        </p>
                      </div>
                    </div>
                  </button>
                ))}
              </div>
            )}
          </TabsContent>
        </div>
      </Tabs>
    </div>
  );
}