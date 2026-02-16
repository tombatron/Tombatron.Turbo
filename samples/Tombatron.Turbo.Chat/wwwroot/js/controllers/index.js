import { application } from "controllers/application"

import ChatController from "controllers/chat"
import TypingIndicatorController from "controllers/typing-indicator"
import ProfileSidebarController from "controllers/profile-sidebar"
import ChatMessageController from "controllers/chat-message"

application.register("chat", ChatController)
application.register("typing-indicator", TypingIndicatorController)
application.register("profile-sidebar", ProfileSidebarController)
application.register("chat-message", ChatMessageController)
