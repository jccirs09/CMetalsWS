import { useState } from "react";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { Input } from "./ui/input";
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from "./ui/sheet";
import { ScrollArea } from "./ui/scroll-area";
import { Avatar, AvatarFallback } from "./ui/avatar";
import {
  MessageCircle,
  ArrowLeft,
  Search,
  Send,
  Phone,
  Video,
  MoreVertical
} from "lucide-react";
import { Messages } from "./Messages";

interface MobileChatProps {
  threadId: string;
  onBack: () => void;
  threadName: string;
  isGroup: boolean;
}

function MobileChat({ threadId, onBack, threadName, isGroup }: MobileChatProps) {
  const [message, setMessage] = useState("");

  return (
    <div className="h-full flex flex-col bg-white">
      {/* Header */}
      <div className="flex items-center gap-3 p-4 border-b bg-gray-50">
        <Button variant="ghost" size="sm" onClick={onBack}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        
        <Avatar className="h-8 w-8">
          <AvatarFallback>{threadName.slice(0, 2).toUpperCase()}</AvatarFallback>
        </Avatar>
        
        <div className="flex-1">
          <h3 className="font-medium text-sm">{threadName}</h3>
          <p className="text-xs text-muted-foreground">
            {isGroup ? "3 members" : "Online"}
          </p>
        </div>
        
        <div className="flex items-center gap-2">
          <Button variant="ghost" size="sm">
            <Phone className="h-4 w-4" />
          </Button>
          <Button variant="ghost" size="sm">
            <Video className="h-4 w-4" />
          </Button>
          <Button variant="ghost" size="sm">
            <MoreVertical className="h-4 w-4" />
          </Button>
        </div>
      </div>

      {/* Messages */}
      <ScrollArea className="flex-1 p-4">
        <div className="space-y-4">
          <div className="flex justify-start">
            <div className="max-w-[70%] bg-gray-100 rounded-lg p-3">
              <p className="text-sm">Hey, how's the production schedule looking for today?</p>
              <span className="text-xs text-muted-foreground">9:00 AM</span>
            </div>
          </div>
          
          <div className="flex justify-end">
            <div className="max-w-[70%] bg-teal-600 text-white rounded-lg p-3">
              <p className="text-sm">Looking good! We're on track to meet the quotas.</p>
              <span className="text-xs opacity-75">9:05 AM</span>
            </div>
          </div>
          
          <div className="flex justify-start">
            <div className="max-w-[70%] bg-gray-100 rounded-lg p-3">
              <p className="text-sm">Perfect! I'll prep the quality check station.</p>
              <span className="text-xs text-muted-foreground">9:10 AM</span>
            </div>
          </div>
        </div>
      </ScrollArea>

      {/* Input */}
      <div className="p-4 border-t">
        <div className="flex items-center gap-2">
          <Input
            value={message}
            onChange={(e) => setMessage(e.target.value)}
            placeholder="Type a message..."
            className="flex-1"
          />
          <Button size="sm" className="bg-teal-600 hover:bg-teal-700">
            <Send className="h-4 w-4" />
          </Button>
        </div>
      </div>
    </div>
  );
}

interface MobileMessagingProps {
  onChatOpen: (threadId: string) => void;
}

export function MobileMessaging({ onChatOpen }: MobileMessagingProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [activeChat, setActiveChat] = useState<string | null>(null);
  const [activeChatName, setActiveChatName] = useState("");
  const [isGroup, setIsGroup] = useState(false);

  const handleChatOpen = (threadId: string) => {
    // Mock data for demo
    const threadData = {
      "t1": { name: "Mike Rodriguez", isGroup: false },
      "t2": { name: "Safety Review", isGroup: true },
      "t3": { name: "David Thompson", isGroup: false }
    };

    const data = threadData[threadId as keyof typeof threadData] || { name: "Chat", isGroup: false };
    
    setActiveChat(threadId);
    setActiveChatName(data.name);
    setIsGroup(data.isGroup);
  };

  const handleBack = () => {
    setActiveChat(null);
  };

  return (
    <Sheet open={isOpen} onOpenChange={setIsOpen}>
      <SheetTrigger asChild>
        <Button variant="ghost" size="sm" className="relative">
          <MessageCircle className="h-4 w-4" />
          <Badge className="absolute -top-1 -right-1 h-5 w-5 rounded-full p-0 text-xs bg-teal-600 text-white">
            2
          </Badge>
        </Button>
      </SheetTrigger>
      
      <SheetContent side="bottom" className="h-[90vh] p-0">
        {activeChat ? (
          <MobileChat
            threadId={activeChat}
            onBack={handleBack}
            threadName={activeChatName}
            isGroup={isGroup}
          />
        ) : (
          <div className="h-full">
            <SheetHeader className="p-4 border-b">
              <SheetTitle>Messages</SheetTitle>
            </SheetHeader>
            <Messages onChatOpen={handleChatOpen} />
          </div>
        )}
      </SheetContent>
    </Sheet>
  );
}